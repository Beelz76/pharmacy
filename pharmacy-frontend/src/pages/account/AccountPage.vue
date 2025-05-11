<template>
  <div>
    <p class="text-red-600">Роль: {{ auth.role }}</p>

    <!-- Личные данные -->
    <el-form :model="form" :rules="rules" ref="formRef" label-position="top">
      <h2 class="text-2xl font-bold mb-6">Личные данные</h2>

      <div class="grid sm:grid-cols-2 gap-4">
        <el-form-item label="ФИО" prop="fullName">
          <el-input
            v-model="form.fullName"
            :readonly="!isEditing"
            placeholder="Фамилия Имя Отчество"
            size="large"
            class="!h-12 !text-base !rounded-md"
            maxlength="100"
            show-word-limit
          />
        </el-form-item>

        <el-form-item label="Телефон">
          <PhoneInput
            v-model="form.phoneFormatted"
            :readonly="!isEditing"
            :required="false"
            :digitsOnly="true"
            size="large"
            class="w-full"
          />
        </el-form-item>
      </div>

      <div v-if="!isEmployee" class="mt-4 flex gap-2">
        <el-button
          v-if="!isEditing"
          type="primary"
          plain
          class="!h-12 text-base"
          @click="startProfileEdit"
        >
          Изменить
        </el-button>
        <template v-else>
          <el-button
            type="success"
            plain
            class="!h-12 text-base"
            :loading="profileLoading"
            :disabled="profileLoading"
            @click="saveChanges"
          >
            Сохранить
          </el-button>
          <el-button
            type="info"
            plain
            class="!h-12 text-base"
            @click="cancelProfileEdit"
          >
            Отмена
          </el-button>
        </template>
      </div>

      <div v-else class="text-sm text-gray-500 mt-1">Для изменения данных обратитесь к администратору</div>
    </el-form>

    <!-- Email -->
    <el-form :model="form" :rules="rules" ref="emailFormRef" label-position="top" class="mt-8">
      <el-form-item label="Email" prop="email">
        <el-input
          v-model="form.email"
          :readonly="!isEditingEmail"
          size="large"
          class="w-full !h-12 !text-base !rounded-md"
        />
      </el-form-item>

      <div v-if="!isEmployee" class="mt-4 flex gap-2">
        <el-button
          v-if="!isEditingEmail"
          type="primary"
          plain
          class="!h-12 text-base"
          @click="startEmailEdit"
        >
          Изменить
        </el-button>
        <template v-else>
          <el-button
            type="success"
            plain
            class="!h-12 text-base"
            :loading="emailLoading"
            :disabled="emailLoading"
            @click="submitEmailChange"
          >
            Сохранить
          </el-button>
          <el-button
            type="info"
            plain
            class="!h-12 text-base"
            @click="cancelEmailEdit"
          >
            Отмена
          </el-button>
        </template>
      </div>

      <div v-else class="text-sm text-gray-500 mt-1">Для изменения email обратитесь к администратору</div>
    </el-form>

    <!-- Изменение пароля -->
    <div v-if="!isEmployee" class="mt-12">
      <h2 class="text-2xl font-bold mb-6">Изменить пароль</h2>
      <el-form :model="form" :rules="rules" ref="passwordFormRef" label-position="top">
        <el-form-item label="Текущий пароль" prop="currentPassword">
          <el-input v-model="form.currentPassword" type="password" size="large" show-password class="!h-12 !text-base !rounded-md" />
        </el-form-item>
        <el-form-item label="Новый пароль" prop="newPassword">
          <el-input v-model="form.newPassword" type="password" size="large" show-password class="!h-12 !text-base !rounded-md" />
        </el-form-item>
        <el-form-item label="Повторить пароль" prop="confirmPassword">
          <el-input v-model="form.confirmPassword" type="password" size="large" show-password class="!h-12 !text-base !rounded-md" />
        </el-form-item>
      </el-form>

      <div class="mt-4">
        <el-button
          type="primary"
          plain
          class="!h-12 text-base"
          :loading="passwordLoading"
          :disabled="passwordLoading"
          @click="submitPasswordChange"
        >
          Изменить пароль
        </el-button>
      </div>
    </div>

    <!-- Модалка подтверждения -->
    <CodeVerificationModal
      v-if="showVerificationModal"
      :visible="showVerificationModal"
      :email="form.email"
      purpose="EmailChange"
      @update:visible="showVerificationModal = $event"
      @success="onEmailChangeConfirmed"
      @close="onVerificationClose"
    />
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { ElMessage } from 'element-plus'
import PhoneInput from '/src/components/inputs/PhoneInput.vue'
import CodeVerificationModal from '/src/components/CodeVerificationModal.vue'
import { useAccountStore } from '/src/store/AccountStore'
import { useAuthStore } from '/src/store/AuthStore'
import api from '/src/utils/axios'

const auth = useAuthStore()
const store = useAccountStore()

const formRef = ref()
const emailFormRef = ref()
const passwordFormRef = ref()

const isEditing = ref(false)
const isEditingEmail = ref(false)
const showVerificationModal = ref(false)

const profileLoading = ref(false)
const emailLoading = ref(false)
const passwordLoading = ref(false)

const originalEmail = ref('')
const originalProfile = ref({ fullName: '', phoneFormatted: '' })

const isEmployee = computed(() => auth.role === 'Employee')

const form = ref({
  fullName: '',
  phoneFormatted: '',
  email: '',
  currentPassword: '',
  newPassword: '',
  confirmPassword: ''
})

const rules = {
  fullName: [
    {
      required: true,
      validator(_, val, cb) {
        if (!val?.trim()) return cb(new Error('ФИО обязательно'))
        cb()
      },
      trigger: 'blur'
    }
  ],
  email: [
    {
      required: true,
      validator(_, val, cb) {
        const trimmed = val?.trim()
        if (!trimmed) return cb(new Error('Email обязателен'))
        const pattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
        return pattern.test(trimmed) ? cb() : cb(new Error('Неверный формат email'))
      },
      trigger: 'blur'
    }
  ],
  currentPassword: [
    {
      required: true,
      validator(_, val, cb) {
        if (!val?.trim()) return cb(new Error('Введите текущий пароль'))
        cb()
      },
      trigger: 'blur'
    }
  ],
  newPassword: [
    {
      required: true,
      validator(_, val, cb) {
        if (!val?.trim()) return cb(new Error('Введите новый пароль'))
        if (val.length < 6) return cb(new Error('Минимум 6 символов'))
        cb()
      },
      trigger: 'blur'
    }
  ],
  confirmPassword: [
    {
      required: true,
      validator(_, val, cb) {
        if (!val?.trim()) return cb(new Error('Подтвердите пароль'))
        if (val !== form.value.newPassword) return cb(new Error('Пароли не совпадают'))
        cb()
      },
      trigger: 'blur'
    }
  ]
}

onMounted(async () => {
  await store.fetchProfile()
  if (store.account) {
    const { lastName, firstName, patronymic, phone, email } = store.account
    const fullName = [lastName, firstName, patronymic].filter(Boolean).join(' ')
    const phoneFormatted = formatPhone(phone || '')
    form.value.fullName = fullName
    form.value.phoneFormatted = phoneFormatted
    form.value.email = email

    originalEmail.value = email
    originalProfile.value = { fullName, phoneFormatted }
  }
})

function startProfileEdit() {
  isEditing.value = true
  originalProfile.value = {
    fullName: form.value.fullName,
    phoneFormatted: form.value.phoneFormatted
  }
}

function cancelProfileEdit() {
  form.value.fullName = originalProfile.value.fullName
  form.value.phoneFormatted = originalProfile.value.phoneFormatted
  isEditing.value = false
}

function startEmailEdit() {
  isEditingEmail.value = true
  originalEmail.value = form.value.email
}

function cancelEmailEdit() {
  form.value.email = originalEmail.value
  isEditingEmail.value = false
}

function onVerificationClose() {
  showVerificationModal.value = false
  isEditingEmail.value = false
}

async function saveChanges() {
  if (!formRef.value) return
  profileLoading.value = true
  try {
    await formRef.value.validate(async (valid) => {
      if (!valid) return
      const [lastName = '', firstName = '', patronymic = ''] = form.value.fullName.trim().split(' ')
      const payload = {
        lastName,
        firstName,
        patronymic,
        phone: form.value.phoneFormatted.replace(/\D/g, '')
      }
      await store.updateProfile(payload)
      ElMessage.success('Данные обновлены')
      isEditing.value = false
    })
  } catch {
    ElMessage.error('Ошибка при сохранении')
  } finally {
    profileLoading.value = false
  }
}

async function submitEmailChange() {
  if (!emailFormRef.value) return
  emailLoading.value = true
  try {
    await emailFormRef.value.validate(async (valid) => {
      if (!valid) return
      await api.post('/users/change-email', { newEmail: form.value.email })
      ElMessage.success('Код отправлен на новую почту')
      showVerificationModal.value = true
    })
  } finally {
    emailLoading.value = false
  }
}

async function submitPasswordChange() {
  if (!passwordFormRef.value) return
  passwordLoading.value = true
  try {
    await passwordFormRef.value.validate(async (valid) => {
      if (!valid) return
      await api.put('/users/change-password', {
        currentPassword: form.value.currentPassword,
        newPassword: form.value.newPassword,
        confirmNewPassword: form.value.confirmPassword
      })
      ElMessage.success('Пароль успешно изменён')
      form.value.currentPassword = ''
      form.value.newPassword = ''
      form.value.confirmPassword = ''
    })
  } catch {
    ElMessage.error('Ошибка при изменении пароля')
  } finally {
    passwordLoading.value = false
  }
}

async function onEmailChangeConfirmed(result) {
  if (result?.data?.token) {
    auth.setToken(result.data.token)
    auth.setEmail(form.value.email)
  }
  isEditingEmail.value = false
  showVerificationModal.value = false
}

function formatPhone(value) {
  const digits = value.replace(/\D/g, '')
  if (digits.length !== 11) return ''
  return `+${digits[0]} (${digits.slice(1, 4)}) ${digits.slice(4, 7)}-${digits.slice(7, 9)}-${digits.slice(9)}`
}
</script>
