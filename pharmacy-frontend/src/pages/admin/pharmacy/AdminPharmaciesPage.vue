<template>
  <div>
    <h1 class="text-2xl font-semibold mb-2">Аптеки</h1>
    <div class="mb-4 text-gray-600">Всего аптек: {{ totalCount }}</div>
    <div class="bg-white rounded-lg shadow p-6 mb-6">
      <el-form :inline="true" @submit.prevent>
        <el-form-item label-width="0">
          <el-input
            v-model="search"
            placeholder="Поиск"
            size="large"
            class="!w-64"
          />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" plain @click="fetch">Поиск</el-button>
          <el-button @click="resetFilters">Сбросить</el-button>
        </el-form-item>
      </el-form>
    </div>
    <div class="flex justify-end mb-4">
      <el-pagination
        layout="sizes, prev, pager, next"
        :total="totalCount"
        :page-size="pageSize"
        :page-sizes="[10, 20, 50]"
        v-model:page-size="pageSize"
        v-model:current-page="pageNumber"
      />
    </div>
    <div class="overflow-x-auto rounded-lg shadow border bg-white">
      <table class="min-w-full table-fixed divide-y divide-gray-200 text-sm">
        <thead
          class="bg-secondary-50 text-left text-secondary-700 uppercase text-sm"
        >
          <tr>
            <th class="px-6 py-5 font-semibold">ID</th>
            <th class="px-6 py-5 font-semibold">Название</th>
            <th class="px-6 py-5 font-semibold">Адрес</th>
            <th class="px-6 py-5 font-semibold text-right">
              <span class="sr-only">Детали</span>
            </th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-100">
          <tr
            v-for="p in pharmacies"
            :key="p.id"
            class="hover:bg-secondary-50 cursor-pointer"
            @click="goDetails(p.id)"
          >
            <td class="px-6 py-4 whitespace-nowrap">{{ p.id }}</td>
            <td class="px-6 py-4 whitespace-nowrap">{{ p.name }}</td>
            <td class="px-6 py-4 whitespace-nowrap">
              {{ formatAddress(p.address) }}
            </td>
            <td class="px-6 py-4 text-right text-gray-400">
              <i class="fas fa-chevron-right"></i>
            </td>
          </tr>
          <tr v-if="!loading && pharmacies.length === 0">
            <td colspan="4" class="text-center py-6 text-gray-500">
              Аптеки не найдены
            </td>
          </tr>
          <tr v-if="loading">
            <td colspan="4" class="text-center py-6 text-gray-500">
              Загрузка...
            </td>
          </tr>
        </tbody>
      </table>
    </div>
    <div class="flex justify-end mt-6">
      <el-pagination
        layout="sizes, prev, pager, next"
        :total="totalCount"
        :page-size="pageSize"
        :page-sizes="[10, 20, 50]"
        v-model:page-size="pageSize"
        v-model:current-page="pageNumber"
      />
    </div>
  </div>
</template>

<script setup>
import { ref, watch } from "vue";
import { useRouter, useRoute } from "vue-router";
import { getPharmacies } from "/src/services/PharmacyService";
import formatAddress from "/src/utils/formatAddress";

const route = useRoute();
const router = useRouter();
const pharmacies = ref([]);
const totalCount = ref(0);
const pageNumber = ref(1);
const pageSize = ref(20);
const loading = ref(false);
const search = ref("");

pageNumber.value = Number(route.query.page) || 1;
pageSize.value = Number(route.query.size) || pageSize.value;
search.value = route.query.search || "";

const fetch = async () => {
  loading.value = true;
  try {
    const data = await getPharmacies({
      page: pageNumber.value,
      size: pageSize.value,
      search: search.value,
    });
    pharmacies.value = data.items;
    totalCount.value = data.totalCount;
  } finally {
    loading.value = false;
  }
};

watch(pageNumber, (val) => {
  router.replace({
    query: { ...route.query, page: val, size: pageSize.value },
  });
  fetch();
});

watch(pageSize, (val) => {
  pageNumber.value = 1;
  router.replace({ query: { ...route.query, page: 1, size: val } });
  fetch();
});

const resetFilters = () => {
  search.value = "";
  pageNumber.value = 1;
  fetch();
};

const goDetails = (id) => {
  router.push({ name: "AdminPharmacyDetails", params: { id } });
};

fetch();
</script>
