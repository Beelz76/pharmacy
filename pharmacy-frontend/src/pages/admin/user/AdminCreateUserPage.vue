<template>
  <div>
    <el-button type="text" @click="router.back()"><i class="fas fa-arrow-left mr-1"></i> Назад</el-button>
    <h1 class="text-2xl font-semibold mb-6">Новый сотрудник</h1>

    <el-form :model="form" :rules="rules" ref="formRef" label-position="top" class="bg-white rounded-lg shadow p-6 max-w-2xl mx-auto">
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
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
        <PhoneInput v-model="form.phone" digits-only size="large" />
      </el-form-item>
      <el-form-item label="Роль" prop="role">
        <el-select v-model="form.role" placeholder="Роль" class="w-full">
          <el-option label="Администратор" value="Admin" />
          <el-option label="Сотрудник" value="Employee" />
        </el-select>
      </el-form-item>
      <el-form-item label="Аптека" prop="pharmacyId" v-if="form.role === 'Employee'" class="md:col-span-2">
        <el-select
          v-model="form.pharmacyId"
          placeholder="Выберите аптеку"
          class="w-full"
          filterable
          remote
          reserve-keyword
          :remote-method="searchPharmacies"
          :loading="loadingPharmacies">
          <el-option v-for="p in pharmacies" :key="p.id" :label="p.name" :value="p.id" />
        </el-select>
      </el-form-item>
      <el-form-item class="md:col-span-2">
        <el-button type="primary" @click="submit" :loading="loading">Создать</el-button>
      </el-form-item>
      </div>
    </el-form>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { createUser } from '/src/services/UserService'
import { getPharmacies } from '/src/services/PharmacyService'
import PhoneInput from '/src/components/inputs/PhoneInput.vue'

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
  role: 'Employee',
  pharmacyId: null
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

const searchPharmacies = async (query = '') => {
  loadingPharmacies.value = true
  try {
    const data = await getPharmacies({ search: query })
    pharmacies.value = data.items
  } finally {
    loadingPharmacies.value = false
  }
}

onMounted(() => searchPharmacies())

watch(
  () => form.value.role,
  (role) => {
    if (role !== 'Employee') {
      form.value.pharmacyId = null
    }
  }
)
</script>