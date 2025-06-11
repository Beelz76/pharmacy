<template>
  <el-dialog
    :model-value="visible"
    @update:model-value="$emit('update:visible', $event)"
    width="420px"
    class="rounded-xl"
    :modal-append-to-body="false"
    :lock-scroll="false"
    align-center
    :append-to-body="false"
    teleported="false"
  >
    <template #header>
      <div class="flex items-center justify-center relative h-10">
        <div
          v-if="step > 1"
          class="absolute left-0 pl-2 cursor-pointer text-gray-500 hover:text-primary-600"
          @click="goBack"
        >
          <i class="fas fa-arrow-left text-base"></i>
        </div>
        <div class="text-xl font-semibold">
          {{ titleByStep }}
        </div>
      </div>
    </template>

    <div class="space-y-4 mt-2">
      <template v-if="step === 1">
        <el-form
          :model="form"
          :rules="rules"
          ref="formRef"
          label-position="top"
          size="large"
        >
          <el-form-item prop="email">
            <el-input
              v-model="form.email"
              :readonly="true"
              placeholder="Email"
              class="!h-11 !text-sm !rounded-md"
            />
          </el-form-item>
          <el-form-item>
            <el-button
              type="primary"
              class="w-full !h-11"
              :loading="loading"
              :disabled="loading"
              @click="sendResetCode"
            >
              Отправить код
            </el-button>
          </el-form-item>
        </el-form>
      </template>

      <template v-else-if="step === 2">
        <div class="flex justify-center gap-2 mb-2">
          <input
            v-for="(digit, index) in codeDigits"
            :key="index"
            :ref="(el) => (codeInputs[index] = el)"
            maxlength="1"
            inputmode="numeric"
            type="text"
            class="w-12 h-12 border rounded-md text-center text-xl"
            v-model="codeDigits[index]"
            @input="onCodeInput(index)"
            @keydown.backspace="onBackspace(index)"
            @paste.prevent="onPaste($event)"
            @keyup.enter="onEnter"
          />
        </div>

        <el-button
          type="primary"
          class="w-full mt-2 !h-11"
          :disabled="verificationCode.length !== 6"
          :loading="loading"
          @click="confirmCode"
        >
          Подтвердить
        </el-button>

        <div class="text-center text-sm text-gray-500 mt-3">
          <template v-if="!canResend">
            Отправить код повторно через {{ resendTimer }} сек
          </template>
          <template v-else>
            <el-button
              class="!h-8 !px-3 !text-sm"
              type="primary"
              text
              size="small"
              :loading="resendLoading"
              :disabled="resendLoading"
              @click="resendCode"
            >
              Отправить код снова
            </el-button>
          </template>
        </div>
      </template>

      <template v-else-if="step === 3">
        <el-form
          :model="form"
          :rules="rules"
          ref="formRef"
          label-position="top"
          size="large"
        >
          <el-form-item prop="newPassword">
            <el-input
              v-model="form.newPassword"
              type="password"
              placeholder="Новый пароль"
              class="!h-11 !rounded-md !text-sm"
            />
          </el-form-item>
          <el-form-item prop="confirmPassword">
            <el-input
              v-model="form.confirmPassword"
              type="password"
              placeholder="Подтвердите пароль"
              class="!h-11 !rounded-md !text-sm"
            />
          </el-form-item>
          <el-form-item>
            <el-button
              type="primary"
              class="w-full !h-11"
              :loading="loading"
              @click="resetPassword"
            >
              Сохранить пароль
            </el-button>
          </el-form-item>
        </el-form>
      </template>
    </div>
  </el-dialog>
</template>

<script setup>
import {
  ref,
  reactive,
  computed,
  nextTick,
  onUnmounted,
  watchEffect,
  watch,
} from "vue";
import { ElMessage } from "element-plus";
import VerificationService from "/src/services/VerificationService.js";
import api from "/src/utils/axios";
import { useAccountStore } from "/src/stores/AccountStore";
import useVerificationCodeInput from "/src/composables/useVerificationCodeInput.js";
import useResendTimer from "/src/composables/useResendTimer.js";

const accountStore = useAccountStore();
const props = defineProps({ visible: Boolean });
const emit = defineEmits(["update:visible"]);

const step = ref(1);
const loading = ref(false);
const resendLoading = ref(false);
const errorMessage = ref("");

const {
  codeInputs,
  codeDigits,
  verificationCode,
  onCodeInput,
  onBackspace,
  onPaste,
  reset: resetCode,
} = useVerificationCodeInput();

const {
  resendTimer,
  canResend,
  start: startResendTimer,
  clear: clearResendTimer,
} = useResendTimer();

const formRef = ref();
const form = reactive({
  email: accountStore.account.email || "",
  newPassword: "",
  confirmPassword: "",
});

const rules = {
  newPassword: [
    { required: true, message: "Введите новый пароль", trigger: "blur" },
    { min: 6, message: "Минимум 6 символов", trigger: "blur" },
  ],
  confirmPassword: [
    {
      required: true,
      validator(_, val, cb) {
        if (!val?.trim()) return cb(new Error("Подтвердите пароль"));
        if (val !== form.newPassword)
          return cb(new Error("Пароли не совпадают"));
        cb();
      },
      trigger: "blur",
    },
  ],
};

const titleByStep = computed(() => {
  return step.value === 1
    ? "Сброс пароля"
    : step.value === 2
    ? "Подтверждение"
    : "Новый пароль";
});

function goBack() {
  if (step.value === 3) step.value = 2;
  else if (step.value === 2) step.value = 1;
}

watchEffect(() => {
  if (props.visible && step.value === 2) {
    resetCode();
    errorMessage.value = "";
    nextTick(() => codeInputs.value[0]?.focus());
    setTimeout(() => {
      startResendTimer();
    }, 50);
  } else {
    clearResendTimer();
  }
});

watch(verificationCode, (val, oldVal) => {
  if (
    props.visible &&
    step.value === 2 &&
    val.length === 6 &&
    oldVal.length !== 6
  ) {
    confirmCode();
  }
});

async function sendResetCode() {
  if (!formRef.value) return;
  await formRef.value.validate();
  loading.value = true;
  try {
    await VerificationService.sendCode(form.email, "PasswordReset");
    ElMessage.success("Код отправлен на почту");
    step.value = 2;
    startResendTimer();
    resetCode();
    nextTick(() => codeInputs.value[0]?.focus());
  } finally {
    loading.value = false;
  }
}

async function resendCode() {
  if (!canResend.value || resendLoading.value) return;
  resendLoading.value = true;
  try {
    await VerificationService.sendCode(form.email, "PasswordReset");
    ElMessage.success("Код повторно отправлен на почту");
    startResendTimer();
    codeDigits.value = ["", "", "", "", "", ""];
    verificationCode.value = "";
    nextTick(() => codeInputs.value[0]?.focus());
  } catch (err) {
    ElMessage.error("Ошибка при отправке кода");
  } finally {
    resendLoading.value = false;
  }
}

async function confirmCode() {
  if (loading.value) return;
  loading.value = true;
  try {
    await VerificationService.confirmCode(
      form.email,
      verificationCode.value,
      "PasswordReset"
    );
    ElMessage.success("Код подтверждён");
    step.value = 3;
  } catch (err) {
    errorMessage.value = err.message || "Ошибка подтверждения";
  } finally {
    loading.value = false;
  }
}

async function resetPassword() {
  await formRef.value.validate();
  loading.value = true;
  try {
    await api.post("/authorization/reset-password", {
      email: form.email,
      newPassword: form.newPassword,
      confirmPassword: form.confirmPassword,
    });
    ElMessage.success("Пароль успешно обновлён");
    emit("update:visible", false);
  } finally {
    loading.value = false;
  }
}

function onEnter() {
  if (verificationCode.value.length === 6) confirmCode();
}

onUnmounted(() => {
  clearResendTimer();
});
</script>
