<template>
  <div>
    <el-button type="text" @click="router.back()"><i class="fas fa-arrow-left mr-1"></i> Назад</el-button>
    <h1 class="text-2xl font-semibold mb-6">Новый сотрудник</h1>

    <el-form :model="form" :rules="rules" ref="formRef" label-position="top" class="bg-white border rounded-xl shadow-sm p-6 max-w-xl">
      <el-form-item label="Email" prop="email">
        <el-input v-model="form.email" placeholder="Email" size="large" />
      </el-form-item>
      <el-form-item label="Пароль" prop="password">
        <el-input v-model="form.password" type="password" size="large" show-password />
      </el-form-item>
      <el-form-item label="Фамилия" prop="lastName">
        <el-input v-model="form.lastName" size="large" />
      </el-form-item>
      <el-form-item label="Имя" prop="firstName">
        <el-input v-model="form.firstName" size="large" />
      </el-form-item>
      <el-form-item label="Отчество">
        <el-input v-model="form.patronymic" size="large" />
      </el-form-item>
      <el-form-item label="Телефон">
        <el-input v-model="form.phone" size="large" />
      </el-form-item>
      <el-form-item label="Роль" prop="role">
        <el-select v-model="form.role" placeholder="Роль" class="w-full">
          <el-option label="Администратор" value="Admin" />
          <el-option label="Сотрудник" value="Employee" />
        </el-select>
      </el-form-item>
      <el-form-item label="Аптека" prop="pharmacyId" v-if="form.role === 'Employee'">
        <el-select v-model="form.pharmacyId" placeholder="Выберите аптеку" class="w-full" :loading="loadingPharmacies">
          <el-option v-for="p in pharmacies" :key="p.id" :label="p.name" :value="p.id" />
        </el-select>
      </el-form-item>
      <el-form-item>
        <el-button type="primary" @click="submit" :loading="loading">Создать</el-button>
      </el-form-item>
    </el-form>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { createUser } from '/src/services/UserService'
import { getPharmacies } from '/src/services/PharmacyService'

const router = useRouter()
const formRef = ref()
const loading = ref(false)
const form = ref({
  email: '',
  password: '',
  firstName: '',
  lastName: '',
  patronymic: '',
  phone: '',
  role: 'Employee'
})

const rules = {
  email: [{ required: true, message: 'Введите email', trigger: 'blur' }],
  password: [{ required: true, message: 'Введите пароль', trigger: 'blur' }],
  firstName: [{ required: true, message: 'Введите имя', trigger: 'blur' }],
  lastName: [{ required: true, message: 'Введите фамилию', trigger: 'blur' }],
  role: [{ required: true, message: 'Выберите роль', trigger: 'change' }],
  pharmacyId: [
    {
      validator(_, val, cb) {
        if (form.value.role === 'Employee' && !val) return cb(new Error('Выберите аптеку'))
        cb()
      },
      trigger: 'change'
    }
  ]
}

const submit = () => {
  formRef.value.validate(async (valid) => {
    if (!valid) return
    loading.value = true
    try {
      await createUser(form.value)
      router.push({ name: 'AdminUsers' })
    } finally {
      loading.value = false
    }
  })
}

const pharmacies = ref([])
const loadingPharmacies = ref(false)

onMounted(async () => {
  loadingPharmacies.value = true
  try {
    pharmacies.value = await getPharmacies()
  } finally {
    loadingPharmacies.value = false
  }
})

watch(
  () => form.value.role,
  (role) => {
    if (role !== 'Employee') {
      form.value.pharmacyId = null
    }
  }
)
</script>