<template>
  <div class="max-w-4xl mx-auto py-10 px-4">
    <!-- Заголовок -->
    <div class="flex items-center gap-3 mb-8">
      <router-link
        :to="{ name: 'OrderCheckout' }"
        class="flex items-center text-primary-600 hover:text-primary-700 text-lg group"
      >
        <i class="fas fa-arrow-left mr-2 group-hover:-translate-x-1 transition-transform duration-150"></i>
        <span>Назад</span>
      </router-link>
      <h2 class="text-2xl font-bold ml-2">Подтверждение заказа</h2>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-2 gap-8">
      <!-- Информация об аптеке и оплате -->
      <div class="bg-white border border-gray-200 rounded-xl p-6 shadow-sm space-y-4">
        <h3 class="text-xl font-semibold text-gray-800 mb-4">Информация о заказе</h3>
        <div v-if="selectedPharmacy">
          <p class="text-sm text-gray-500">Аптека</p>
          <p class="text-base font-medium text-gray-800">{{ selectedPharmacy.name }}</p>
          <p class="text-sm text-gray-600">{{ selectedPharmacy.address }}</p>
        </div>
        <div v-if="paymentMethod">
          <p class="text-sm text-gray-500 mt-4">Способ оплаты</p>
          <p class="text-base font-medium text-gray-800">
            {{ paymentMethod === 'Online' ? 'Картой онлайн' : 'При получении' }}
          </p>
        </div>
      </div>

      <!-- Состав заказа -->
      <div class="bg-white border border-gray-200 rounded-xl p-6 shadow-sm">
        <h3 class="text-xl font-semibold text-gray-800 mb-4">Состав заказа</h3>
        <div class="divide-y divide-gray-100">
          <div
            v-for="item in cartItems"
            :key="item.productId"
            class="py-4 flex justify-between items-start"
          >
            <div>
              <p class="font-medium text-gray-900">{{ item.name }}</p>
              <p class="text-sm text-gray-500 mt-0.5">{{ item.description }}</p>
            </div>
            <div class="text-right">
              <p class="text-sm text-gray-500">×{{ item.quantity }}</p>
              <p class="text-base font-semibold text-gray-800">{{ item.totalPrice.toFixed(2) }} ₽</p>
            </div>
          </div>
        </div>
        <div class="text-right mt-6 text-xl font-bold text-gray-900 border-t pt-4">
          Итого: {{ totalPrice.toFixed(2) }} ₽
        </div>
      </div>
    </div>

    <!-- Кнопка подтверждения -->
    <div class="text-right mt-8">
      <el-button
        type="primary"
        size="large"
        :loading="loading"
        @click="submitOrder"
        class="!bg-primary-600 hover:!bg-primary-700 !px-10 !py-3 rounded-lg text-base"
      >
        Подтвердить заказ
      </el-button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { useCartStore } from '../../stores/CartStore'
import { useOrderStore } from '../../stores/OrderStore'
import api from '../../utils/axios'

const router = useRouter()
const cartStore = useCartStore()
const orderStore = useOrderStore()

const loading = ref(false)

const selectedPharmacy = orderStore.selectedPharmacy
const paymentMethod = orderStore.paymentMethod
const cartItems = cartStore.items
const totalPrice = cartStore.totalPrice

const fullPharmacyAddress = computed(() =>
  selectedPharmacy ? selectedPharmacy.address : ''
)

const submitOrder = async () => {
  try {
    loading.value = true

    const response = await api.post('/orders', {
      pharmacyAddress: `${selectedPharmacy.address}, ${selectedPharmacy.name}`,
      paymentMethod
    })

    cartStore.resetCart()
    const { id, number } = response.data

    orderStore.setCreatedOrder({
      id,
      number,
      total: totalPrice
    })

    if (paymentMethod === 'OnDelivery') {
      orderStore.resetOrder()
      router.push({ name: 'OrderHistory' })
    } else {
      router.push({ name: 'OrderPayment' })
    }
  } catch (error) {
    console.error('Ошибка при оформлении заказа:', error)
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.el-button {
  transition: background-color 0.2s ease-in-out;
}
</style>
