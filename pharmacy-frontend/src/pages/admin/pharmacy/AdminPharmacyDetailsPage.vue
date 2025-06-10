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
    <div v-if="loading" class="text-center py-10">Загрузка...</div>
    <div v-else-if="!pharmacy" class="text-center py-10 text-gray-500">
      Аптека не найдена
    </div>
    <div v-else class="bg-white border rounded-xl p-6 shadow-sm space-y-3">
      <p class="text-base text-gray-700">
        <span class="font-medium">Название:</span> {{ pharmacy.name }}
      </p>
      <p class="text-base text-gray-700">
        <span class="font-medium">Телефон:</span> {{ pharmacy.phone || "—" }}
      </p>
      <p class="text-base text-gray-700">
        <span class="font-medium">Адрес:</span>
        {{ formatAddress(pharmacy.address) }}
      </p>
      <div class="text-left pt-2">
        <el-button type="primary" @click="goWarehouse">Склад</el-button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import { useRoute, useRouter } from "vue-router";
import { getPharmacyById } from "/src/services/PharmacyService";
import formatAddress from "/src/utils/formatAddress";

const route = useRoute();
const router = useRouter();
const pharmacy = ref(null);
const loading = ref(false);

const goWarehouse = () => {
  if (pharmacy.value) {
    router.push({
      name: "AdminPharmacyWarehouse",
      params: { id: pharmacy.value.id },
    });
  }
};

onMounted(async () => {
  loading.value = true;
  try {
    pharmacy.value = await getPharmacyById(route.params.id);
  } catch {
    pharmacy.value = null;
  } finally {
    loading.value = false;
  }
});
</script>
