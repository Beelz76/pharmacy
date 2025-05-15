<template>
  <div class="max-w-7xl mx-auto">
    <h2 class="text-2xl font-bold mb-6">История заказов</h2>

    <div v-if="loading" class="text-center py-10">
      <LoadingSpinner size="lg" />
    </div>

    <div v-else>
      <div class="overflow-x-auto rounded-xl shadow-sm border">
        <table class="min-w-full divide-y divide-gray-200 bg-white">
          <thead class="bg-gray-50">
            <tr class="text-left text-sm text-gray-600">
              <th class="px-4 py-3 font-semibold">
                <i class="fas fa-hashtag mr-2 text-gray-400"></i>Номер
              </th>
              <th class="px-4 py-3 font-semibold">
                <i class="fas fa-calendar-alt mr-2 text-gray-400"></i>Дата
              </th>
              <th class="px-4 py-3 font-semibold">
                <i class="fas fa-ruble-sign mr-2 text-gray-400"></i>Сумма
              </th>
              <th class="px-4 py-3 font-semibold">
                <i class="fas fa-info-circle mr-2 text-gray-400"></i>Статус
              </th>
              <th class="px-4 py-3 font-semibold">
                <i class="fas fa-key mr-2 text-gray-400"></i>Код получения
              </th>
            </tr>
          </thead>
          <tbody class="divide-y divide-gray-100 text-sm">
            <tr
              v-for="order in orders"
              :key="order.id"
              class="hover:bg-gray-50 transition cursor-pointer"
              @click="goToOrder(order.id)"
            >
              <td class="px-4 py-3 font-medium text-gray-900" @mousedown.stop>{{ order.number }}</td>
              <td class="px-4 py-3 text-gray-700" @mousedown.stop>{{ formatDate(order.createdAt) }}</td>
              <td class="px-4 py-3 text-gray-700 whitespace-nowrap" @mousedown.stop>
                {{ order.totalPrice.toFixed(2) }} ₽
              </td>
              <td class="px-4 py-3" @mousedown.stop>
                <span
                  class="inline-block px-2 py-0.5 rounded-full text-xs font-medium"
                  :class="statusClass(order.status)"
                >
                  {{ order.status }}
                </span>

              </td>
              <td class="px-4 py-3 text-gray-700" @mousedown.stop>
                {{ order.pickupCode || '—' }}
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
import { ref, onMounted, watch } from 'vue'
import { useAuthStore } from '/src/stores/AuthStore'
import { useOrders } from '/src/composables/useOrders'
import LoadingSpinner from '/src/components/LoadingSpinner.vue'
import { useRouter } from 'vue-router'
import { useOrderNavigationStore } from '/src/stores/OrderStore'

const navStore = useOrderNavigationStore()
const router = useRouter()
const auth = useAuthStore()
const pageSize = 5
const pageNumber = ref(1)
const { orders, totalCount, loading, fetchOrders } = useOrders()

const goToOrder = (orderId) => {
  router.push({
    name: 'OrderDetails',
    params: { id: orderId },
    query: { page: pageNumber.value }
  })
}

const formatDate = (isoString) => {
  return new Date(isoString).toLocaleString('ru-RU', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

const statusClass = (status) => {
  switch (status) {
    case 'Ожидает оплаты':
    case 'Ожидает обработки':
      return 'bg-yellow-100 text-yellow-800'
    case 'В обработке':
    case 'Готов к получению':
      return 'bg-blue-100 text-blue-800'
    case 'Получен':
      return 'bg-green-100 text-green-700'
    case 'Отменен':
    case 'Возврат средств':
      return 'bg-red-100 text-red-700'
    default:
      return 'bg-gray-100 text-gray-600'
  }
}

onMounted(() => {
  const shouldRestore = navStore.consumeRestoreFlag()
  if (shouldRestore) {
    pageNumber.value = navStore.historyPage
  } else {
    pageNumber.value = 1
  }

  fetchOrders({
    userId: auth.user?.id,
    page: pageNumber.value,
    size: pageSize
  })
})

watch(pageNumber, (val) => {
  fetchOrders({
    userId: auth.user?.id,
    page: val,
    size: pageSize
  })
})
</script>

