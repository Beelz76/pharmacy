<template>
  <div>
    <h1 class="text-2xl font-semibold mb-6">Доставки</h1>

    <div class="bg-white rounded-lg shadow p-6 mb-6">
      <el-form :inline="true" @submit.prevent>
        <el-form-item label-width="0">
          <el-input
            v-model="filters.orderNumber"
            placeholder="Номер заказа"
            size="large"
            class="!w-40"
          />
        </el-form-item>
        <el-form-item label="Период">
          <el-date-picker
            v-model="filters.dateRange"
            type="daterange"
            range-separator="-"
            start-placeholder="От"
            end-placeholder="До"
          />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" @click="fetch"
            ><i class="fas fa-search mr-1" /> Поиск</el-button
          >
          <el-button @click="resetFilters"
            ><i class="fas fa-sync-alt mr-1" />Сбросить</el-button
          >
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
            <th class="px-6 py-5 font-semibold">Заказ</th>
            <th class="px-6 py-5 font-semibold">Адрес</th>
            <th class="px-6 py-5 font-semibold">Дата</th>
            <th class="px-6 py-5 font-semibold text-right">
              <span class="sr-only">Детали</span>
            </th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-100">
          <tr
            v-for="d in deliveries"
            :key="d.id"
            class="hover:bg-secondary-50 cursor-pointer"
            @click="goDetails(d.orderId)"
          >
            <td class="px-6 py-4 whitespace-nowrap">{{ d.id }}</td>
            <td class="px-6 py-4 whitespace-nowrap">{{ d.orderNumber }}</td>
            <td class="px-6 py-4 whitespace-nowrap">{{ d.address }}</td>
            <td class="px-6 py-4 whitespace-nowrap">
              {{ formatDate(d.deliveryDate) }}
            </td>
            <td class="px-6 py-4 text-right text-gray-400">
              <i class="fas fa-chevron-right"></i>
            </td>
          </tr>
          <tr v-if="!loading && deliveries.length === 0">
            <td colspan="5" class="text-center py-6 text-gray-500">
              Доставки не найдены
            </td>
          </tr>
          <tr v-if="loading">
            <td colspan="5" class="text-center py-6 text-gray-500">
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
import { ref, watch, reactive } from "vue";
import { useRouter, useRoute } from "vue-router";
import { getDeliveries } from "/src/services/DeliveryService";

const route = useRoute();
const router = useRouter();
const deliveries = ref([]);
const totalCount = ref(0);
const pageNumber = ref(1);
const pageSize = ref(20);
const loading = ref(false);
const filters = reactive({ orderNumber: "", dateRange: [] });
pageNumber.value = Number(route.query.page) || 1;
pageSize.value = Number(route.query.size) || pageSize.value;

const fetch = async () => {
  loading.value = true;
  try {
    const payload = {
      orderNumber: filters.orderNumber || null,
      fromDate: filters.dateRange?.[0] || null,
      toDate: filters.dateRange?.[1] || null,
    };
    const data = await getDeliveries({
      page: pageNumber.value,
      size: pageSize.value,
      filters: payload,
    });
    deliveries.value = data.items;
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
  filters.orderNumber = "";
  filters.dateRange = [];
  pageNumber.value = 1;
  fetch();
};

const formatDate = (iso) => (iso ? new Date(iso).toLocaleString("ru-RU") : "");

const goDetails = (orderId) => {
  router.push({
    name: "AdminDeliveryDetails",
    params: { orderId },
    query: { page: pageNumber.value },
  });
};

fetch();
</script>
