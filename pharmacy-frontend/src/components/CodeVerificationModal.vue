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
        <div class="text-xl font-semibold">Подтверждение</div>
      </div>
    </template>

    <div class="space-y-4 mt-2">
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
    </div>
  </el-dialog>
</template>

<script setup>
import { ref, watchEffect, onUnmounted } from "vue";
import { ElMessage } from "element-plus";
import VerificationService from "/src/services/VerificationService.js";
import useVerificationCodeInput from "/src/composables/useVerificationCodeInput.js";
import useResendTimer from "/src/composables/useResendTimer.js";

const props = defineProps({
  visible: Boolean,
  email: String,
  purpose: String,
});

const emit = defineEmits(["close", "success", "update:visible"]);

const resendLoading = ref(false);
const loading = ref(false);
const errorMessage = ref("");

const {
  codeInputs,
  codeDigits,
  verificationCode,
  onCodeInput,
  onBackspace,
  onPaste,
  reset,
} = useVerificationCodeInput();

const {
  resendTimer,
  canResend,
  start: startResendTimer,
  clear: clearResendTimer,
} = useResendTimer();

watchEffect(() => {
  if (props.visible) {
    reset();
    errorMessage.value = "";
    startResendTimer();
  } else {
    clearResendTimer();
  }
});

async function resendCode() {
  if (!canResend.value || resendLoading.value) return;
  resendLoading.value = true;
  try {
    await VerificationService.sendCode(props.email, props.purpose);
    ElMessage.success("Код отправлен на почту");
    startResendTimer();
    reset();
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
    const result = await VerificationService.confirmCode(
      props.email,
      verificationCode.value,
      props.purpose
    );
    ElMessage.success("Код подтверждён");

    emit("success", result);
    emit("close");
  } catch (err) {
    errorMessage.value = err.message || "Ошибка подтверждения";
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
