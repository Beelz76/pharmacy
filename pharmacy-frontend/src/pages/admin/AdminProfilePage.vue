<template>
  <div>
    <h1 class="text-2xl font-semibold mb-6">Профиль администратора</h1>
    <div class="grid lg:grid-cols-2 gap-8">
      <!-- Персональные данные -->
      <el-card class="!p-6 border rounded-xl">
        <template #header>
          <h2 class="text-xl font-semibold text-gray-900">
            Персональные данные
          </h2>
        </template>
        <el-form
          :model="form"
          :rules="rules"
          ref="formRef"
          label-position="top"
        >
          <div class="grid sm:grid-cols-2 gap-4">
            <el-form-item label="ФИО" prop="fullName">
              <el-input
                v-model="form.fullName"
                :readonly="!isEditing"
                placeholder="Фамилия Имя Отчество"
                size="large"
                class="!h-12 !rounded-md"
                maxlength="100"
                show-word-limit
              />
            </el-form-item>
            <el-form-item label="Телефон">
              <PhoneInput
                v-model="form.phoneFormatted"
                :readonly="!isEditing"
                :required="false"
                :digits-only="true"
                size="large"
                class="w-full"
              />
            </el-form-item>
          </div>
          <div class="mt-6 flex justify-end gap-3">
            <el-button
              v-if="!isEditing"
              @click="startProfileEdit"
              type="primary"
              plain
            >
              <i class="fas fa-edit mr-1"></i> Изменить
            </el-button>
            <template v-else>
              <el-button
                @click="saveChanges"
                type="success"
                plain
                :loading="profileLoading"
              >
                <i class="fas fa-check mr-1"></i> Сохранить
              </el-button>
              <el-button @click="cancelProfileEdit" type="info" plain
                >Отмена</el-button
              >
            </template>
          </div>

          <template #header>
            <h2 class="text-xl font-semibold text-gray-900">Почта</h2>
          </template>
          <el-form
            :model="form"
            :rules="rules"
            ref="emailFormRef"
            label-position="top"
          >
            <el-form-item label="Email" prop="email">
              <el-input
                v-model="form.email"
                :readonly="!isEditingEmail"
                size="large"
                class="w-full !h-12 !rounded-md"
              />
            </el-form-item>
            <div class="mt-6 flex justify-end gap-3">
              <el-button
                v-if="!isEditingEmail"
                @click="startEmailEdit"
                type="primary"
                plain
              >
                <i class="fas fa-edit mr-1"></i> Изменить
              </el-button>
              <template v-else>
                <el-button
                  @click="submitEmailChange"
                  type="success"
                  plain
                  :loading="emailLoading"
                >
                  <i class="fas fa-check mr-1"></i> Сохранить
                </el-button>
                <el-button @click="cancelEmailEdit" type="info" plain
                  >Отмена</el-button
                >
              </template>
            </div>
          </el-form>
        </el-form>
      </el-card>

      <!-- Почта и пароль -->
      <div class="space-y-8">
        <!-- Пароль -->
        <el-card class="!p-6 border rounded-xl">
          <template #header>
            <div class="flex justify-between items-center">
              <h2 class="text-xl font-semibold text-gray-900">
                Изменить пароль
              </h2>
              <button
                class="text-sm text-primary-600 hover:underline"
                @click="showPasswordResetModal = true"
              >
                Забыли пароль?
              </button>
            </div>
          </template>
          <el-form
            :model="form"
            :rules="rules"
            ref="passwordFormRef"
            label-position="top"
          >
            <el-form-item label="Текущий пароль" prop="currentPassword">
              <el-input
                v-model="form.currentPassword"
                type="password"
                show-password
                size="large"
                class="!h-12 !rounded-md"
              />
            </el-form-item>
            <el-form-item label="Новый пароль" prop="newPassword">
              <el-input
                v-model="form.newPassword"
                type="password"
                show-password
                size="large"
                class="!h-12 !rounded-md"
              />
            </el-form-item>
            <el-form-item label="Повторить пароль" prop="confirmPassword">
              <el-input
                v-model="form.confirmPassword"
                type="password"
                show-password
                @paste.prevent
                size="large"
                class="!h-12 !rounded-md"
              />
            </el-form-item>
            <div class="mt-6 flex justify-end">
              <el-button
                @click="submitPasswordChange"
                type="primary"
                plain
                :loading="passwordLoading"
              >
                <i class="fas fa-key mr-1"></i> Изменить пароль
              </el-button>
            </div>
          </el-form>
        </el-card>
      </div>
    </div>

    <!-- Модалки -->
    <CodeVerificationModal
      v-if="showVerificationModal"
      :visible="showVerificationModal"
      :email="form.email"
      purpose="EmailChange"
      @update:visible="showVerificationModal = $event"
      @success="onEmailChangeConfirmed"
      @close="onVerificationClose"
    />

    <PasswordResetModal
      v-if="showPasswordResetModal"
      :visible="showPasswordResetModal"
      :email="form.email"
      @update:visible="showPasswordResetModal = $event"
    />
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import { ElMessage } from "element-plus";
import { storeToRefs } from "pinia";
import PhoneInput from "/src/components/inputs/PhoneInput.vue";
import CodeVerificationModal from "/src/components/CodeVerificationModal.vue";
import PasswordResetModal from "/src/components/PasswordResetModal.vue";
import { useAccountStore } from "/src/stores/AccountStore";
import { useAuthStore } from "/src/stores/AuthStore";
import api from "/src/utils/axios";

const auth = useAuthStore();
const accountStore = useAccountStore();
const { account } = storeToRefs(accountStore);

const formRef = ref();
const emailFormRef = ref();
const passwordFormRef = ref();

const isEditing = ref(false);
const isEditingEmail = ref(false);
const showVerificationModal = ref(false);
const showPasswordResetModal = ref(false);

const profileLoading = ref(false);
const emailLoading = ref(false);
const passwordLoading = ref(false);

const originalEmail = ref("");
const originalProfile = ref({ fullName: "", phoneFormatted: "" });

const form = ref({
  fullName: "",
  phoneFormatted: "",
  email: "",
  currentPassword: "",
  newPassword: "",
  confirmPassword: "",
});

const rules = {
  fullName: [
    {
      required: true,
      validator(_, val, cb) {
        if (!val?.trim()) return cb(new Error("ФИО обязательно"));
        cb();
      },
      trigger: "blur",
    },
  ],
  email: [
    {
      required: true,
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
  currentPassword: [
    {
      required: true,
      validator(_, val, cb) {
        if (!val?.trim()) return cb(new Error("Введите текущий пароль"));
        cb();
      },
      trigger: "blur",
    },
  ],
  newPassword: [
    {
      required: true,
      validator(_, val, cb) {
        if (!val?.trim()) return cb(new Error("Введите новый пароль"));
        if (val.length < 6) return cb(new Error("Минимум 6 символов"));
        cb();
      },
      trigger: "blur",
    },
  ],
  confirmPassword: [
    {
      required: true,
      validator(_, val, cb) {
        if (!val?.trim()) return cb(new Error("Подтвердите пароль"));
        if (val !== form.value.newPassword)
          return cb(new Error("Пароли не совпадают"));
        cb();
      },
      trigger: "blur",
    },
  ],
};

onMounted(async () => {
  await accountStore.fetchProfile();
  if (account.value) {
    const { lastName, firstName, patronymic, phone, email } = account.value;
    const fullName = [lastName, firstName, patronymic]
      .filter(Boolean)
      .join(" ");
    const phoneFormatted = formatPhone(phone || "");
    form.value.fullName = fullName;
    form.value.phoneFormatted = phoneFormatted;
    form.value.email = email;

    originalEmail.value = email;
    originalProfile.value = { fullName, phoneFormatted };
  }
});

function startProfileEdit() {
  isEditing.value = true;
  originalProfile.value = {
    fullName: form.value.fullName,
    phoneFormatted: form.value.phoneFormatted,
  };
}

function cancelProfileEdit() {
  form.value.fullName = originalProfile.value.fullName;
  form.value.phoneFormatted = originalProfile.value.phoneFormatted;
  isEditing.value = false;
}

function startEmailEdit() {
  isEditingEmail.value = true;
  originalEmail.value = form.value.email;
}

function cancelEmailEdit() {
  form.value.email = originalEmail.value;
  isEditingEmail.value = false;
}

function onVerificationClose() {
  showVerificationModal.value = false;
  isEditingEmail.value = false;
}

async function saveChanges() {
  if (!formRef.value) return;
  profileLoading.value = true;
  try {
    await formRef.value.validate(async (valid) => {
      if (!valid) return;
      const [lastName = "", firstName = "", patronymic = ""] =
        form.value.fullName.trim().split(" ");
      const payload = {
        lastName,
        firstName,
        patronymic,
        phone: form.value.phoneFormatted.replace(/\D/g, ""),
      };
      await accountStore.updateProfile(payload);
      ElMessage.success("Данные обновлены");
      isEditing.value = false;
    });
  } catch {
    ElMessage.error("Ошибка при сохранении");
  } finally {
    profileLoading.value = false;
  }
}

async function submitEmailChange() {
  if (!emailFormRef.value) return;
  emailLoading.value = true;
  try {
    await emailFormRef.value.validate(async (valid) => {
      if (!valid) return;
      await api.post("/users/change-email", { newEmail: form.value.email });
      ElMessage.success("Код отправлен на новую почту");
      showVerificationModal.value = true;
    });
  } finally {
    emailLoading.value = false;
  }
}

async function submitPasswordChange() {
  if (!passwordFormRef.value) return;
  passwordLoading.value = true;
  try {
    await passwordFormRef.value.validate(async (valid) => {
      if (!valid) return;
      await api.put("/users/change-password", {
        currentPassword: form.value.currentPassword,
        newPassword: form.value.newPassword,
        confirmNewPassword: form.value.confirmPassword,
      });
      ElMessage.success("Пароль успешно изменён");
      form.value.currentPassword = "";
      form.value.newPassword = "";
      form.value.confirmPassword = "";
    });
  } catch {
    ElMessage.error("Ошибка при изменении пароля");
  } finally {
    passwordLoading.value = false;
  }
}

async function onEmailChangeConfirmed(result) {
  if (result?.data?.token) {
    auth.setToken(result.data.token);
    auth.setEmail(form.value.email);
  }
  isEditingEmail.value = false;
  showVerificationModal.value = false;
}

function formatPhone(value) {
  const digits = value.replace(/\D/g, "");
  if (digits.length !== 11) return "";
  return `+${digits[0]} (${digits.slice(1, 4)}) ${digits.slice(
    4,
    7
  )}-${digits.slice(7, 9)}-${digits.slice(9)}`;
}
</script>
