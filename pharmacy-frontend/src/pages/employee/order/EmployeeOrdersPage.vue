<template>
  <div>
    <h1 class="text-2xl font-semibold mb-2">
      Заказы
      <template v-if="user"> пользователя {{ user.email }} </template>
    </h1>
    <div class="mb-4 text-gray-600">Всего заказов: {{ totalCount }}</div>

    <div class="bg-white rounded-lg shadow p-6 mb-6">
      <el-form :inline="true" @submit.prevent>
        <div class="flex flex-wrap items-end gap-4 mb-4">
          <!-- Основные фильтры -->
          <el-form-item label-width="0">
            <el-input
              v-model="filters.number"
              placeholder="Номер"
              size="large"
              class="!w-40"
            />
          </el-form-item>
          <el-form-item label-width="0">
            <el-input
              v-model="filters.userEmail"
              placeholder="Email"
              size="large"
              class="!w-48"
            />
          </el-form-item>
          <el-form-item label-width="0">
            <el-input
              v-model="filters.userFullName"
              placeholder="ФИО"
              size="large"
              class="!w-52"
            />
          </el-form-item>

          <!-- Статус и период -->
          <el-form-item label="Статус">
            <el-select
              v-model="filters.status"
              placeholder="Все"
              clearable
              class="!w-40"
            >
              <el-option
                v-for="s in statuses"
                :key="s.id"
                :label="s.description"
                :value="s.name"
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

          <el-form-item label="Цена от">
            <el-input-number v-model="filters.minPrice" :min="0" />
          </el-form-item>
          <el-form-item label="до">
            <el-input-number v-model="filters.maxPrice" :min="0" />
          </el-form-item>

          <!-- Кнопки действий -->
          <el-form-item>
            <el-button type="primary" @click="fetch"
              ><i class="fas fa-search mr-1" /> Поиск</el-button
            >
            <el-button @click="resetFilters">
              <i class="fas fa-sync-alt mr-1" />Сбросить</el-button
            >
          </el-form-item>
        </div>
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
            <th class="px-6 py-5 font-semibold">Номер</th>
            <th class="px-6 py-5 font-semibold">Дата</th>
            <th class="px-6 py-5 font-semibold">Сумма</th>
            <th class="px-6 py-5 font-semibold">Статус</th>
            <th class="px-6 py-5 font-semibold">Покупатель</th>
            <th class="px-6 py-5 font-semibold">Код</th>
            <th class="px-6 py-5 font-semibold text-right">
              <span class="sr-only">Детали</span>
            </th>
          </tr>
        </thead>
        <tbody class="divide-y divide-gray-100">
          <tr
            v-for="o in orders"
            :key="o.id"
            class="hover:bg-secondary-50 cursor-pointer"
            @click="goDetails(o.id)"
          >
            <td class="px-6 py-4 whitespace-nowrap">{{ o.id }}</td>
            <td class="px-6 py-4 whitespace-nowrap">{{ o.number }}</td>
            <td class="px-6 py-4 whitespace-nowrap">
              {{ formatDate(o.createdAt) }}
            </td>
            <td class="px-6 py-4 whitespace-nowrap">
              {{ o.totalPrice.toFixed(2) }} ₽
            </td>
            <td class="px-6 py-4 whitespace-nowrap" @click.stop>
              <el-select
                v-model="o.status"
                size="small"
                @change="changeStatus(o)"
              >
                <el-option
                  v-for="s in statuses"
                  :key="s.id"
                  :label="s.description"
                  :value="s.name"
                />
              </el-select>
            </td>
            <td class="px-6 py-4 whitespace-nowrap">{{ o.userFullName }}</td>
            <td class="px-6 py-4 whitespace-nowrap">
              {{ o.pickupCode || "—" }}
            </td>
            <td class="px-6 py-4 text-right text-gray-400">
              <i class="fas fa-chevron-right"></i>
            </td>
          </tr>
          <tr v-if="!loading && orders.length === 0">
            <td colspan="8" class="text-center py-6 text-gray-500">
              Заказы не найдены
            </td>
          </tr>
          <tr v-if="loading">
            <td colspan="8" class="text-center py-6 text-gray-500">
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
import { reactive, watch, ref } from "vue";
import { useOrders } from "/src/composables/useOrders";
import { useRouter, useRoute } from "vue-router";
import { getUserById } from "/src/services/UserService";
import { useAccountStore } from "/src/stores/AccountStore";
import {
  getOrderStatuses,
  updateOrderStatus,
} from "/src/services/OrderService";
import { ElMessage } from "element-plus";

const route = useRoute();
const router = useRouter();
const user = ref(null);
const accountStore = useAccountStore();
accountStore.fetchProfile();
const filters = reactive({
  number: "",
  userId: null,
  userEmail: "",
  userFullName: "",
  pharmacyId: null,
  status: null,
  dateRange: [],
  minPrice: null,
  maxPrice: null,
});

const { orders, totalCount, pageNumber, pageSize, loading, fetchOrders } =
  useOrders();

pageNumber.value = Number(route.query.page) || 1;
pageSize.value = Number(route.query.size) || pageSize.value;
filters.userId = route.query.userId ? Number(route.query.userId) : null;

const loadUser = async (id) => {
  if (!id) {
    user.value = null;
    return;
  }
  try {
    user.value = await getUserById(id);
  } catch {
    user.value = null;
  }
};
loadUser(filters.userId);

const statuses = ref([]);

const changeStatus = async (order) => {
  try {
    await updateOrderStatus(order.id, order.status);
    ElMessage.success("Статус обновлен");
  } catch (e) {
    ElMessage.error("Ошибка обновления статуса");
  }
};

const loadStatuses = async () => {
  statuses.value = await getOrderStatuses();
};

function fetch() {
  const payload = {
    number: filters.number || null,
    userId: filters.userId || null,
    userEmail: filters.userEmail || null,
    userFullName: filters.userFullName || null,
    pharmacyId: filters.pharmacyId || null,
    status: filters.status || null,
    fromDate: filters.dateRange?.[0] || null,
    toDate: filters.dateRange?.[1] || null,
    fromPrice: filters.minPrice,
    toPrice: filters.maxPrice,
  };
  fetchOrders({
    page: pageNumber.value,
    size: pageSize.value,
    filters: payload,
  });
}

const resetFilters = () => {
  filters.number = "";
  filters.userId = null;
  filters.userEmail = "";
  filters.userFullName = "";
  filters.pharmacyId = null;
  filters.status = null;
  filters.dateRange = [];
  filters.minPrice = null;
  filters.maxPrice = null;
  pageNumber.value = 1;
  fetch();
};

watch(
  () => accountStore.account,
  (val) => {
    if (val?.pharmacy?.id) {
      filters.pharmacyId = val.pharmacy.id;
      fetch();
    }
  },
  { immediate: true }
);

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

watch(
  () => route.query.userId,
  (val) => {
    filters.userId = val ? Number(val) : null;
    loadUser(filters.userId);
    pageNumber.value = 1;
    fetch();
  }
);

const formatDate = (iso) => {
  return new Date(iso).toLocaleString("ru-RU", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
};

const goDetails = (id) => {
  router.push({
    name: "EmployeeOrderDetails",
    params: { id },
    query: { page: pageNumber.value },
  });
};

loadStatuses();
fetch();
</script>
