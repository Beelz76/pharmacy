<template>
  <el-dialog
    :model-value="visible"
    @update:model-value="emit('update:visible', $event)"
    width="420px"
    class="rounded-xl"
    :modal-append-to-body="false"
    :lock-scroll="false"
    align-center
    :append-to-body="false"
    teleported="false"
  >
    <!-- Заголовок -->
    <template #header>
      <div class="flex items-center justify-center relative h-10">
        <div
          v-if="isPasswordReset || showVerification || showResetPasswordFields"
          class="absolute left-0 pl-2 cursor-pointer text-gray-500 hover:text-primary-600"
          @click="goBack"
        >
          <i class="fas fa-arrow-left text-base"></i>
        </div>
        <div class="text-xl font-semibold">
          {{
            isPasswordReset
              ? showResetPasswordFields
                ? "Новый пароль"
                : showVerification
                ? "Подтверждение"
                : "Восстановление пароля"
              : isLogin
              ? showVerification
                ? "Подтверждение"
                : "Вход"
              : showVerification
              ? "Подтверждение"
              : "Регистрация"
          }}
        </div>
      </div>
    </template>

    <div class="space-y-4 mt-2">
      <!-- Этап 1: Ввод email для сброса -->
      <template v-if="showResetStep1">
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
              placeholder="Email"
              class="!h-11 !rounded-md"
            />
          </el-form-item>
          <el-form-item>
            <el-button
              type="primary"
              class="w-full !h-11"
              :loading="loading"
              @click="() => validateAnd(sendResetCode)"
            >
              Отправить код
            </el-button>
          </el-form-item>
        </el-form>
      </template>

      <!-- Этап 2: Подтверждение кода -->
      <template v-else-if="showVerification && !showResetPasswordFields">
        <div class="flex justify-center gap-2 mb-2">
          <input
            v-for="(digit, index) in codeDigits"
            :key="index"
            ref="codeInputs"
            maxlength="1"
            inputmode="numeric"
            type="text"
            class="w-12 h-12 border rounded-md text-center text-xl"
            v-model="codeDigits[index]"
            @input="onCodeInput(index)"
            @keydown.backspace="onBackspace(index)"
            @paste.prevent="onPaste($event)"
            @keyup.enter="onEnterFromCode"
          />
        </div>
        <el-button
          type="primary"
          class="w-full !h-11"
          :disabled="verificationCode.length !== 6 || loading"
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
            <button
              class="text-primary-600 hover:underline"
              :disabled="loading"
              @click="resendCode"
            >
              Отправить код снова
            </button>
          </template>
        </div>
      </template>

      <!-- Этап 3: Смена пароля -->
      <template v-else-if="showResetPasswordFields">
        <el-form
          :model="form"
          :rules="rules"
          ref="formRef"
          label-position="top"
          size="large"
          @keyup.enter="handleSubmit"
        >
          <el-form-item prop="newPassword">
            <el-input
              v-model="form.newPassword"
              type="password"
              placeholder="Новый пароль"
              class="!h-11 !rounded-md"
              show-password
            />
          </el-form-item>
          <el-form-item prop="confirmPassword">
            <el-input
              v-model="form.confirmPassword"
              type="password"
              placeholder="Подтверждение пароля"
              class="!h-11 !rounded-md"
              show-password
              @paste.prevent
            />
          </el-form-item>
          <el-form-item>
            <el-button
              type="primary"
              class="w-full !h-11"
              :loading="loading"
              @click="() => validateAnd(resetPassword)"
            >
              Сохранить пароль
            </el-button>
          </el-form-item>
        </el-form>
      </template>

      <!-- Этап 4: Авторизация / Регистрация -->
      <template v-else>
        <el-form
          :model="form"
          :rules="rules"
          ref="formRef"
          label-position="top"
          size="large"
          @keyup.enter="handleSubmit"
        >
          <el-form-item prop="email">
            <el-input
              v-model="form.email"
              placeholder="Email"
              class="!h-11 !rounded-md"
            />
          </el-form-item>

          <el-form-item prop="password">
            <el-input
              v-model="form.password"
              :type="showPassword ? 'text' : 'password'"
              placeholder="Пароль"
              class="!h-11 !rounded-md"
            >
              <template #suffix>
                <i
                  :class="showPassword ? 'fas fa-eye-slash' : 'fas fa-eye'"
                  class="cursor-pointer text-gray-500"
                  @click="showPassword = !showPassword"
                ></i>
              </template>
            </el-input>
          </el-form-item>

          <div v-if="isLogin" class="text-left text-sm -mt-2 mb-3">
            <button
              type="button"
              class="text-primary-600 hover:underline"
              @click="startPasswordReset"
            >
              Забыли пароль?
            </button>
          </div>

          <!-- Регистрация -->
          <template v-if="!isLogin">
            <el-form-item prop="lastName">
              <el-input
                v-model="form.lastName"
                placeholder="Фамилия"
                class="!h-11 !rounded-md"
              />
            </el-form-item>
            <el-form-item prop="firstName">
              <el-input
                v-model="form.firstName"
                placeholder="Имя"
                class="!h-11 !rounded-md"
              />
            </el-form-item>
            <el-form-item>
              <el-input
                v-model="form.patronymic"
                placeholder="Отчество (необязательно)"
                class="!h-11 !rounded-md"
              />
            </el-form-item>
            <PhoneInput
              v-model="form.phone"
              placeholder="Телефон (необязательно)"
              :required="false"
              :wrapWithFormItem="false"
              :digitsOnly="true"
              size="large"
            />
            <el-form-item prop="agreedToPolicy" class="!mt-2">
              <el-checkbox
                v-model="form.agreedToPolicy"
                class="!text-sm whitespace-normal"
              >
                Согласен(а) с
                <router-link
                  to="/privacy-policy"
                  target="_blank"
                  class="text-primary-600 hover:underline"
                >
                  политикой конфиденциальности
                </router-link>
                и обработкой персональных данных
              </el-checkbox>
            </el-form-item>
          </template>

          <el-form-item>
            <el-button
              type="primary"
              class="w-full !h-11 mt-4"
              :loading="loading"
              @click="handleSubmit"
            >
              {{ isLogin ? "Войти" : "Зарегистрироваться" }}
            </el-button>
          </el-form-item>
        </el-form>

        <!-- Переключение -->
        <div class="text-center text-sm text-gray-500 mt-2">
          {{ isLogin ? "Нет аккаунта?" : "Уже есть аккаунт?" }}
          <button
            class="text-primary-600 hover:underline ml-1"
            :disabled="loading"
            @click="toggleMode"
          >
            {{ isLogin ? "Зарегистрироваться" : "Войти" }}
          </button>
        </div>
      </template>
    </div>
  </el-dialog>
</template>

<script setup>
import { ref, reactive, nextTick, watch, computed, onUnmounted } from "vue";
import api from "../utils/axios";
import { ElMessage } from "element-plus";
import PhoneInput from "/src/components/inputs/PhoneInput.vue";
import VerificationService from "/src/services/VerificationService.js";
import { useAuthStore } from "../stores/AuthStore";
import { useRouter } from "vue-router";
import useVerificationCodeInput from "/src/composables/useVerificationCodeInput.js";
import useResendTimer from "/src/composables/useResendTimer.js";

const router = useRouter();
const auth = useAuthStore();
const props = defineProps({ visible: Boolean });
const emit = defineEmits(["update:visible"]);

const isLogin = ref(true);
const isPasswordReset = ref(false);
const showPassword = ref(false);
const showVerification = ref(false);
const showResetPasswordFields = ref(false);
const loading = ref(false);
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

onUnmounted(() => {
  clearResendTimer();
});

const showResetStep1 = computed(
  () =>
    isPasswordReset.value &&
    !showVerification.value &&
    !showResetPasswordFields.value
);

const formRef = ref();
const form = reactive({
  email: "",
  password: "",
  lastName: "",
  firstName: "",
  patronymic: "",
  phone: "",
  agreedToPolicy: false,
  newPassword: "",
  confirmPassword: "",
});

const rules = {
  email: [
    {
      validator(_, val, cb) {
        const trimmed = val?.trim();
        if (!trimmed) return cb(new Error("Email обязателен"));
        const pattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return pattern.test(trimmed)
          ? cb()
          : cb(new Error("Неверный формат email"));
      },
      trigger: "blur",
    },
  ],
  password: [
    {
      validator(_, val, cb) {
        const trimmed = val?.trim();
        if (!trimmed) return cb(new Error("Пароль обязателен"));
        return trimmed.length >= 6
          ? cb()
          : cb(new Error("Пароль должен быть не менее 6 символов"));
      },
      trigger: "blur",
    },
  ],
  firstName: [
    {
      validator(_, val, cb) {
        return val?.trim() ? cb() : cb(new Error("Имя обязательно"));
      },
      trigger: "blur",
    },
  ],
  lastName: [
    {
      validator(_, val, cb) {
        return val?.trim() ? cb() : cb(new Error("Фамилия обязательна"));
      },
      trigger: "blur",
    },
  ],
  newPassword: [
    {
      validator(_, val, cb) {
        if (!val?.trim()) return cb();
        return val.length >= 6
          ? cb()
          : cb(new Error("Пароль должен быть не менее 6 символов"));
      },
      trigger: "blur",
    },
  ],
  confirmPassword: [
    {
      validator(_, val, cb) {
        if (!val?.trim()) return cb();
        if (val !== form.newPassword)
          return cb(new Error("Пароли не совпадают"));
        return cb();
      },
      trigger: "blur",
    },
  ],
  agreedToPolicy: [
    {
      validator(_, val, cb) {
        if (isLogin.value) return cb();
        return val ? cb() : cb(new Error("Необходимо согласие с политикой"));
      },
      trigger: "change",
    },
  ],
};

watch(
  () => props.visible,
  (val) => {
    if (!val) resetAll();
  }
);

watch(verificationCode, (val, oldVal) => {
  if (
    props.visible &&
    showVerification.value &&
    !showResetPasswordFields.value &&
    val.length === 6 &&
    oldVal.length !== 6
  ) {
    confirmCode();
  }
});

function validateAnd(callback) {
  if (!formRef.value) return;
  formRef.value.validate((valid) => {
    if (valid) callback();
  });
}

function toggleMode() {
  isLogin.value = !isLogin.value;
  isPasswordReset.value = false;
  showVerification.value = false;
  showResetPasswordFields.value = false;
  nextTick(resetFormFields);
}

function startPasswordReset() {
  isPasswordReset.value = true;
  isLogin.value = false;
  showVerification.value = false;
  showResetPasswordFields.value = false;
  nextTick(resetFormFields);
}

function resetFormFields() {
  Object.keys(form).forEach((k) => (form[k] = ""));
  errorMessage.value = "";
  resetCode();
  nextTick(() => formRef.value?.clearValidate?.());
}

function resetAll() {
  clearResendTimer();
  isLogin.value = true;
  isPasswordReset.value = false;
  showPassword.value = false;
  showVerification.value = false;
  showResetPasswordFields.value = false;
  loading.value = false;
  resetFormFields();
}

function handleSubmit() {
  formRef.value.validate((valid) => {
    if (valid) isLogin.value ? submitLogin() : submitRegister();
  });
}

async function submitLogin() {
  loading.value = true;
  try {
    const res = await api.post("/authorization/login", {
      email: form.email,
      password: form.password,
    });
    const token = res.data?.token;
    const rToken = res.data?.refreshToken;
    if (token && rToken) {
      auth.setToken(token);
      auth.setRefreshToken(rToken);
      emit("update:visible", false);
      nextTick(() => {
        let target = auth.returnUrl;
        if (!target || target === "/") {
          target =
            auth.role === "Admin"
              ? "/admin"
              : auth.role === "Employee"
              ? "/employee"
              : "/";
        }
        router.push(target);
        auth.clearReturnUrl();
      });
    } else {
      await sendCode("Registration");
      showVerification.value = true;
    }
  } finally {
    loading.value = false;
  }
}

async function submitRegister() {
  loading.value = true;
  try {
    await api.post("/authorization/register-initial", {
      email: form.email,
      password: form.password,
      firstName: form.firstName,
      lastName: form.lastName,
      patronymic: form.patronymic || undefined,
      phone: form.phone || undefined,
    });
    showVerification.value = true;
    startResendTimer();
    resetCode();
  } finally {
    loading.value = false;
  }
}

async function sendResetCode() {
  loading.value = true;
  try {
    errorMessage.value = "";
    await VerificationService.sendCode(form.email, "PasswordReset");
    ElMessage.success("Код отправлен на почту");
    showVerification.value = true;
    startResendTimer();
    resetCode();
  } finally {
    loading.value = false;
  }
}

async function sendCode(purpose) {
  loading.value = true;
  try {
    errorMessage.value = "";
    await VerificationService.sendCode(form.email, purpose);
    ElMessage.success("Код отправлен на почту");
    startResendTimer();
    resetCode();
  } finally {
    loading.value = false;
  }
}

async function resendCode() {
  if (!canResend.value) return;
  errorMessage.value = "";
  await sendCode(isPasswordReset.value ? "PasswordReset" : "Registration");
}

async function confirmCode() {
  loading.value = true;
  try {
    const result = await VerificationService.confirmCode(
      form.email,
      verificationCode.value,
      isPasswordReset.value ? "PasswordReset" : "Registration"
    );
    if (result.data?.token && result.data?.refreshToken) {
      auth.setToken(result.data.token);
      auth.setRefreshToken(result.data.refreshToken);
      emit("update:visible", false);
      nextTick(() => {
        let target = auth.returnUrl;
        if (!target || target === "/") {
          target =
            auth.role === "Admin"
              ? "/admin"
              : auth.role === "Employee"
              ? "/employee"
              : "/";
        }
        router.push(target);
        auth.clearReturnUrl();
      });
    } else {
      ElMessage.success("Код подтверждён");
      if (isPasswordReset.value) {
        showResetPasswordFields.value = true;
        showVerification.value = false;
      }
    }
  } catch (err) {
    errorMessage.value = err.message || "Ошибка подтверждения";
  } finally {
    loading.value = false;
  }
}

function onEnterFromCode() {
  if (verificationCode.value.length === 6) confirmCode();
}

async function resetPassword() {
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

function goBack() {
  if (showResetPasswordFields.value) {
    showResetPasswordFields.value = false;
    showVerification.value = true;
  } else if (showVerification.value) {
    showVerification.value = false;
    if (isPasswordReset.value) isPasswordReset.value = true;
  } else if (isPasswordReset.value) {
    isPasswordReset.value = false;
    isLogin.value = true;
  }
  resetCode();
}
</script>
