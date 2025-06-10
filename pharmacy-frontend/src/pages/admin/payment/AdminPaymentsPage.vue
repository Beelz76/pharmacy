<template>
  <div>
    <h1 class="text-2xl font-semibold mb-6">Платежи</h1>

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
        <el-form-item label="Статус">
          <el-select
            v-model="filters.status"
            placeholder="Все"
            clearable
            class="!w-48"
          >
            <el-option
              v-for="s in statuses"
              :key="s.id"
              :label="s.description"
              :value="s.name"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="Метод">
          <el-select
            v-model="filters.method"
            placeholder="Все"
            clearable
            class="!w-48"
          >
            <el-option
              v-for="m in methods"
              :key="m.id"
              :label="m.description"
              :value="m.name"
            />
          </el-select>
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
            <th class="px-6 py-5 font-semibold">Заказ</th>
            <th class="px-6 py-5 font-semibold">Аптека</th>
            <th class="px-6 py-5 font-semibold">Сумма</th>
            <th class="px-6 py-5 font-semibold">Метод</th>
            <th class="px-6 py-5 font-semibold">Статус</th>
            <th class="px-6 py-5 font-semibold text-right">
              <span class="sr-only">Детали</span>
            </th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-100">
          <tr
            v-for="p in payments"
            :key="p.id"
            class="hover:bg-secondary-50 cursor-pointer"
            @click="goDetails(p.id)"
          >
            <td class="px-6 py-4 whitespace-nowrap">{{ p.id }}</td>
            <td class="px-6 py-4 whitespace-nowrap">{{ p.orderNumber }}</td>
            <td class="px-6 py-4 whitespace-nowrap">{{ p.pharmacyName }}</td>
            <td class="px-6 py-4 whitespace-nowrap">
              {{ p.amount.toFixed(2) }} ₽
            </td>
            <td class="px-6 py-4 whitespace-nowrap">{{ p.method }}</td>
            <td class="px-6 py-4 whitespace-nowrap">{{ p.status }}</td>
            <td class="px-6 py-4 text-right text-gray-400">
              <i class="fas fa-chevron-right"></i>
            </td>
          </tr>
          <tr v-if="!loading && payments.length === 0">
            <td colspan="7" class="text-center py-6 text-gray-500">
              Платежи не найдены
            </td>
          </tr>
          <tr v-if="loading">
            <td colspan="7" class="text-center py-6 text-gray-500">
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
import { getPayments } from "/src/services/PaymentService";
import {
  getPaymentMethods,
  getPaymentStatuses,
} from "/src/services/PaymentReferenceService";

const router = useRouter();
const route = useRoute();

const payments = ref([]);
const totalCount = ref(0);
const pageNumber = ref(1);
const pageSize = ref(20);
const loading = ref(false);

const filters = reactive({
  orderNumber: "",
  status: null,
  method: null,
  dateRange: [],
});

const statuses = ref([]);
const methods = ref([]);

pageNumber.value = Number(route.query.page) || 1;
pageSize.value = Number(route.query.size) || pageSize.value;

const loadRefs = async () => {
  statuses.value = await getPaymentStatuses();
  methods.value = await getPaymentMethods();
};

const fetch = async () => {
  loading.value = true;
  try {
    const payload = {
      orderNumber: filters.orderNumber || null,
      status: filters.status || null,
      method: filters.method || null,
      fromDate: filters.dateRange?.[0] || null,
      toDate: filters.dateRange?.[1] || null,
    };
    const data = await getPayments({
      page: pageNumber.value,
      size: pageSize.value,
      filters: payload,
    });
    payments.value = data.items;
    totalCount.value = data.totalCount;
  } finally {
    loading.value = false;
  }
};

const resetFilters = () => {
  filters.orderNumber = "";
  filters.status = null;
  filters.method = null;
  filters.dateRange = [];
  pageNumber.value = 1;
  fetch();
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

const goDetails = (id) => {
  router.push({
    name: "AdminPaymentDetails",
    params: { id },
    query: { page: pageNumber.value },
  });
};

loadRefs();
fetch();
</script>
