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
      <h2 class="text-2xl font-bold tracking-tight">
        Пользователь #{{ user?.id }}
      </h2>
    </div>

    <div v-if="loading" class="text-center py-10">Загрузка...</div>
    <div v-else-if="!user" class="text-center py-10 text-gray-500">
      Пользователь не найден
    </div>
    <div v-else class="bg-white border rounded-xl p-6 shadow-sm">
      <el-form :model="form" :rules="rules" ref="formRef" label-position="top">
        <div class="grid md:grid-cols-2 gap-4">
          <el-form-item label="Email" prop="email">
            <el-input v-model="form.email" size="large" />
          </el-form-item>
          <el-form-item label="Роль" prop="role">
            <el-select v-model="form.role" class="w-full">
              <el-option label="Администратор" value="Admin" />
              <el-option label="Сотрудник" value="Employee" />
            </el-select>
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
        <div class="mt-4 flex justify-end">
          <el-button type="primary" @click="submit" :loading="saving">
            Сохранить
          </el-button>
          <el-button
            v-if="user.role === 'User'"
            @click="goToUserOrders(user.id)"
            class="ml-3"
            >Заказы пользователя</el-button
          >
        </div>
      </el-form>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from "vue";
import { useRoute, useRouter } from "vue-router";
import { ElMessage } from "element-plus";
import { getUserById, updateUser } from "/src/services/UserService";
import { getPharmacies } from "/src/services/PharmacyService";
import PhoneInput from "/src/components/inputs/PhoneInput.vue";
import formatAddress from "/src/utils/formatAddress";

const route = useRoute();
const router = useRouter();
const user = ref(null);
const loading = ref(false);
const formRef = ref();
const saving = ref(false);
const form = ref({
  email: "",
  firstName: "",
  lastName: "",
  patronymic: "",
  phone: "",
  role: "Employee",
  pharmacyId: null,
});

const rules = {
  email: [{ required: true, message: "Введите email", trigger: "blur" }],
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

const pharmacyNames = ref([]);
const pharmacyAddresses = ref([]);
const selectedPharmacyName = ref(null);
const pharmacyOptions = ref([]);
const loadingPharmacies = ref(false);

onMounted(async () => {
  loading.value = true;
  try {
    const data = await getUserById(route.params.id);
    user.value = data;
    if (data) {
      form.value.email = data.email;
      form.value.firstName = data.firstName;
      form.value.lastName = data.lastName;
      form.value.patronymic = data.patronymic;
      form.value.phone = data.phone || "";
      form.value.role = data.role;
      form.value.pharmacyId = data.pharmacy?.id || null;
      selectedPharmacyName.value = data.pharmacy?.name || null;
    }
    searchPharmacyNames();
    if (selectedPharmacyName.value) searchPharmacyAddresses();
  } catch (e) {
    user.value = null;
  } finally {
    loading.value = false;
  }
});

const goToUserOrders = (id) => {
  router.push({ name: "AdminOrders", query: { userId: id } });
};

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

watch(
  () => form.value.role,
  (role) => {
    if (role !== "Employee") {
      form.value.pharmacyId = null;
      selectedPharmacyName.value = null;
      pharmacyAddresses.value = [];
    }
  }
);

watch(selectedPharmacyName, () => {
  form.value.pharmacyId = null;
  searchPharmacyAddresses();
});

const submit = () => {
  if (!formRef.value) return;
  formRef.value.validate(async (valid) => {
    if (!valid) return;
    saving.value = true;
    try {
      await updateUser(route.params.id, { ...form.value });
      ElMessage.success("Данные обновлены");
    } finally {
      saving.value = false;
    }
  });
};
</script>
