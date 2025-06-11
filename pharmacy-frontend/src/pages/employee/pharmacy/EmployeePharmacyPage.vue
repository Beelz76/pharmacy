<template>
  <div class="max-w-3xl mx-auto">
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
        Аптека #{{ pharmacy?.id }}
      </h2>
    </div>

    <div v-if="loading" class="text-center py-10 text-gray-500 text-lg">
      <i class="fas fa-spinner fa-spin mr-2"></i>Загрузка информации...
    </div>

    <div v-else-if="!pharmacy" class="text-center py-10 text-gray-500 text-lg">
      <i class="fas fa-exclamation-circle mr-2"></i>Аптека не найдена
    </div>

    <div v-else class="bg-white border rounded-xl p-6 shadow-sm space-y-6">
      <div
        class="grid grid-cols-1 sm:grid-cols-2 gap-y-4 gap-x-8 text-gray-700"
      >
        <p><span class="font-medium">Название:</span> {{ pharmacy.name }}</p>
        <p>
          <span class="font-medium">Телефон:</span> {{ pharmacy.phone || "—" }}
        </p>
        <p class="sm:col-span-2">
          <span class="font-medium">Адрес:</span>
          {{ formatAddress(pharmacy.address) }}
        </p>
      </div>

      <div class="pt-4 text-right">
        <el-button type="primary" @click="goWarehouse">
          <i class="fas fa-boxes mr-2"></i>Перейти к складу
        </el-button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import { useRouter } from "vue-router";
import { getPharmacyById } from "/src/services/PharmacyService";
import { useAccountStore } from "/src/stores/AccountStore";
import formatAddress from "/src/utils/formatAddress";

const router = useRouter();
const accountStore = useAccountStore();
accountStore.fetchProfile();
const pharmacy = ref(null);
const loading = ref(false);

const goWarehouse = () => {
  if (pharmacy.value) {
    router.push({ name: "EmployeeWarehouse" });
  }
};

onMounted(async () => {
  loading.value = true;
  try {
    if (accountStore.account?.pharmacy?.id) {
      pharmacy.value = await getPharmacyById(accountStore.account.pharmacy.id);
    }
  } catch {
    pharmacy.value = null;
  } finally {
    loading.value = false;
  }
});
</script>
