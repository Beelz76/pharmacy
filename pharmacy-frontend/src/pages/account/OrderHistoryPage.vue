<template>
  <div class="max-w-7xl mx-auto">
    <h2 class="text-2xl font-bold mb-6">История заказов</h2>

    <div v-if="!auth.isAuthenticated" class="text-gray-500 text-center">
      Необходимо авторизоваться
    </div>

    <div v-else-if="auth.isAuthenticated && loading" class="text-center py-10">
      <LoadingSpinner size="lg" />
    </div>

    <div
      v-else-if="auth.isAuthenticated && orders.length === 0"
      class="text-center text-lg text-gray-500 py-10"
    >
      У вас пока нет заказов
    </div>

    <div v-else-if="auth.isAuthenticated && orders.length > 0">
      <div class="overflow-x-auto rounded-xl shadow border">
        <table
          class="min-w-full table-auto divide-y divide-gray-200 bg-white text-sm"
        >
          <thead class="bg-gray-50">
            <tr
              class="text-left text-gray-600 uppercase text-sm tracking-wider"
            >
              <th class="w-[160px] px-6 py-6 font-semibold text-gray-600">
                Номер
              </th>
              <th class="w-[180px] px-6 py-6 font-semibold text-gray-600">
                Дата
              </th>
              <th class="w-[120px] px-6 py-6 font-semibold text-gray-600">
                Сумма
              </th>
              <th class="w-[180px] px-6 py-6 font-semibold text-gray-600">
                Статус
              </th>
              <th class="w-[140px] px-6 py-6 font-semibold text-gray-600">
                Код
              </th>
              <th class="w-[40px] px-6 py-6 font-semibold text-right">
                <span class="sr-only">Детали</span>
              </th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100">
            <tr
              v-for="order in orders"
              :key="order.id"
              class="hover:bg-gray-50 transition cursor-pointer group text-[15px] leading-relaxed"
              @click="goToOrder(order.id)"
            >
              <td
                class="px-6 py-6 font-medium text-gray-900 whitespace-nowrap"
                @mousedown.stop
              >
                {{ order.number }}
              </td>
              <td
                class="px-6 py-6 text-gray-700 whitespace-nowrap"
                @mousedown.stop
              >
                {{ formatDate(order.createdAt) }}
              </td>
              <td
                class="px-6 py-6 text-gray-700 whitespace-nowrap"
                @mousedown.stop
              >
                {{ order.totalPrice.toFixed(2) }} ₽
              </td>
              <td class="px-6 py-6 whitespace-nowrap" @mousedown.stop>
                <span
                  class="inline-block w-full text-center px-3 py-2 rounded-full text-xs font-semibold"
                  :class="statusClass(order.status)"
                >
                  {{ order.status }}
                </span>
              </td>
              <td
                class="px-6 py-6 text-gray-700 whitespace-nowrap text-center"
                @mousedown.stop
              >
                {{ order.pickupCode || "—" }}
              </td>
              <td
                class="px-6 py-6 text-right text-gray-400 group-hover:text-primary-600 transition w-8"
              >
                <i class="fas fa-chevron-right"></i>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div v-if="totalCount > pageSize" class="flex justify-center mt-10">
        <el-pagination
          layout="prev, pager, next"
          :total="totalCount"
          :page-size="pageSize"
          v-model:current-page="pageNumber"
          class="!text-xl scale-110"
        />
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch } from "vue";
import { useAuthStore } from "/src/stores/AuthStore";
import { useOrders } from "/src/composables/useOrders";
import LoadingSpinner from "/src/components/LoadingSpinner.vue";
import { useRouter, useRoute } from "vue-router";
import { statusClass } from "../../utils/statusClass";

const router = useRouter();
const route = useRoute();
const auth = useAuthStore();
const pageSize = 5;
const pageNumber = ref(1);
const { orders, totalCount, loading, fetchOrders } = useOrders();

const goToOrder = (orderId) => {
  router.push({
    name: "OrderDetails",
    params: { id: orderId },
    query: { page: pageNumber.value },
  });
};

const formatDate = (isoString) => {
  return new Date(isoString).toLocaleString("ru-RU", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
};

onMounted(() => {
  if (!auth.isAuthenticated) return;
  pageNumber.value = Number(route.query.page) || 1;
  fetchOrders({ page: pageNumber.value, size: pageSize });
});

watch(
  () => auth.isAuthenticated,
  (val) => {
    if (val) {
      fetchOrders({ page: pageNumber.value, size: pageSize });
    }
  }
);

watch(pageNumber, (val) => {
  router.replace({ query: { ...route.query, page: val } });
  if (auth.isAuthenticated) {
    fetchOrders({ page: val, size: pageSize });
  }
});
</script>
