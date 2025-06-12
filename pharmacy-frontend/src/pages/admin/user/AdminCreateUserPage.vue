<template>
  <div class="max-w-5xl mx-auto">
    <div class="flex items-center gap-4 mb-8">
      <button
        @click="router.back()"
        class="flex items-center text-primary-600 hover:text-primary-700 text-base group transition"
      >
        <i
          class="text-xl fas fa-arrow-left mr-2 group-hover:-translate-x-1 duration-150"
        ></i>
      </button>
      <h2 class="text-2xl font-bold tracking-tight">Новый сотрудник</h2>
    </div>

    <div class="bg-white border rounded-xl p-6 shadow-sm">
      <el-form
        :model="form"
        :rules="rules"
        ref="formRef"
        label-position="top"
        class="space-y-4"
      >
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <el-form-item label="Email" prop="email">
            <el-input v-model="form.email" placeholder="Email" size="large" />
          </el-form-item>
          <el-form-item label="Пароль" prop="password">
            <el-input
              v-model="form.password"
              type="password"
              size="large"
              show-password
            />
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

          <el-form-item
            label="Аптека"
            v-if="form.role === 'Employee'"
            class="md:col-span-2"
          >
            <el-select
              v-model="selectedPharmacyName"
              placeholder="Выберите аптеку"
              class="w-full"
              clearable
              filterable
              remote
              reserve-keyword
              :remote-method="searchPharmacyNames"
              :loading="loadingPharmacies"
            >
              <el-option
                v-for="n in pharmacyNames"
                :key="n"
                :label="n"
                :value="n"
              />
            </el-select>
          </el-form-item>

          <el-form-item
            label="Адрес"
            prop="pharmacyId"
            v-if="form.role === 'Employee'"
            class="md:col-span-2"
          >
            <el-select
              v-model="form.pharmacyId"
              placeholder="Адрес"
              class="w-full"
              filterable
              remote
              reserve-keyword
              :remote-method="searchPharmacyAddresses"
              :loading="loadingPharmacies"
            >
              <el-option
                v-for="p in pharmacyAddresses"
                :key="p.id"
                :label="p.address"
                :value="p.id"
              />
            </el-select>
          </el-form-item>
        </div>
        <div class="text-right">
          <el-button type="primary" @click="submit" :loading="loading">
            Создать
          </el-button>
        </div>
      </el-form>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from "vue";
import { useRouter } from "vue-router";
import { createUser } from "/src/services/UserService";
import { getPharmacies } from "/src/services/PharmacyService";
import formatAddress from "/src/utils/formatAddress";
import PhoneInput from "/src/components/inputs/PhoneInput.vue";

const router = useRouter();
const formRef = ref();
const loading = ref(false);
const form = ref({
  email: "",
  password: "",
  firstName: "",
  lastName: "",
  patronymic: "",
  phone: "",
  role: "Employee",
  pharmacyId: null,
});

const rules = {
  email: [{ required: true, message: "Введите email", trigger: "blur" }],
  password: [{ required: true, message: "Введите пароль", trigger: "blur" }],
  firstName: [{ required: true, message: "Введите имя", trigger: "blur" }],
  lastName: [{ required: true, message: "Введите фамилию", trigger: "blur" }],
  role: [{ required: true, message: "Выберите роль", trigger: "change" }],
  pharmacyId: [
    {
      validator(_, val, cb) {
        if (form.value.role === "Employee" && !val)
          return cb(new Error("Выберите аптеку"));
        cb();
      },
      trigger: "change",
    },
  ],
};

const submit = () => {
  formRef.value.validate(async (valid) => {
    if (!valid) return;
    loading.value = true;
    try {
      await createUser(form.value);
      router.push({ name: "AdminUsers" });
    } finally {
      loading.value = false;
    }
  });
};

const pharmacyNames = ref([]);
const pharmacyAddresses = ref([]);
const selectedPharmacyName = ref(null);
const loadingPharmacies = ref(false);

const searchPharmacyNames = async (query = "") => {
  loadingPharmacies.value = true;
  try {
    const data = await getPharmacies({ search: query });
    const names = Array.from(new Set(data.items.map((p) => p.name)));
    pharmacyNames.value = names;
  } finally {
    loadingPharmacies.value = false;
  }
};

const searchPharmacyAddresses = async (query = "") => {
  if (!selectedPharmacyName.value) {
    pharmacyAddresses.value = [];
    return;
  }
  loadingPharmacies.value = true;
  try {
    const data = await getPharmacies({ search: selectedPharmacyName.value });
    let list = data.items.filter((p) => p.name === selectedPharmacyName.value);
    if (query) {
      const q = query.toLowerCase();
      list = list.filter((p) =>
        formatAddress(p.address).toLowerCase().includes(q)
      );
    }
    pharmacyAddresses.value = list.map((p) => ({
      id: p.id,
      address: formatAddress(p.address),
    }));
  } finally {
    loadingPharmacies.value = false;
  }
};

onMounted(() => searchPharmacyNames());

watch(
  () => form.value.role,
  (role) => {
    if (role !== "Employee") {
      form.value.pharmacyId = null;
      selectedPharmacyName.value = null;
      pharmacyAddresses.value = [];
      formRef.value?.clearValidate("pharmacyId");
    }
  }
);

watch(selectedPharmacyName, () => {
  form.value.pharmacyId = null;
  searchPharmacyAddresses();
  formRef.value?.clearValidate("pharmacyId");
});
</script>
