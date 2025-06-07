<template>
  <div class="max-w-4xl mx-auto py-10 px-4">
    <!-- Назад и заголовок -->
    <div class="flex items-center gap-4 mb-8">
      <router-link
        :to="{ name: 'OrderCheckout' }"
        class="flex items-center text-primary-600 hover:text-primary-700 text-base group"
      >
        <i class="text-xl fas fa-arrow-left mr-2 group-hover:-translate-x-1 duration-150"></i>
      </router-link>
      <h2 class="text-2xl font-bold tracking-tight">Подтверждение заказа</h2>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-2 gap-8">
      <!-- Информация о заказе -->
      <div class="bg-white border border-gray-200 rounded-xl p-6 shadow-sm space-y-5">
        <h3 class="text-lg font-semibold text-gray-800">
          <i class="fas fa-file-alt mr-2 text-gray-400"></i>Информация о заказе
        </h3>

        <template v-if="isDelivery">
          <p class="text-sm text-gray-500 uppercase tracking-wide">Адрес доставки</p>
          <p class="text-base font-medium text-gray-900">{{ selectedAddress?.fullAddress }}</p>
        </template>
        <template v-else>
          <p class="text-sm text-gray-500 uppercase tracking-wide">Аптека</p>
          <p class="text-base font-medium text-gray-900">«{{ selectedPharmacy.name }}»</p>
          <p class="text-sm text-gray-600">{{ selectedPharmacy.address }}</p>
        </template>

        <div v-if="paymentMethod" class="pt-4">
          <p class="text-sm text-gray-500 uppercase tracking-wide">Оплата</p>
          <p class="text-base font-medium text-gray-900">
            {{ paymentMethod === 'Online' ? 'Картой онлайн' : 'При получении' }}
          </p>
        </div>
      </div>

<!-- Состав заказа -->
<div class="bg-white border border-gray-200 rounded-xl p-6 shadow-sm space-y-5">
  <h3 class="text-lg font-semibold text-gray-800">
    <i class="fas fa-box mr-2 text-gray-400"></i>Состав заказа
  </h3>

  <!-- Прокручиваемый список -->
  <div class="custom-scroll divide-y divide-gray-100 max-h-[360px] overflow-y-auto pr-3 -mr-3">
  <div
    v-for="item in cartItems"
    :key="item.productId"
    class="py-4 flex justify-between items-start text-sm"
  >
      <div class="pr-4">
        <p class="font-medium text-gray-900">{{ item.name }}</p>
        <p class="text-gray-500 mt-1">{{ item.description }}</p>
      </div>
      <div class="text-right whitespace-nowrap">
        <p class="text-gray-500">×{{ item.quantity }}</p>
        <p class="font-semibold text-gray-900 text-base">
          {{ item.totalPrice.toFixed(2) }} ₽
        </p>
      </div>
    </div>
  </div>

  <div class="pt-4 border-t text-right text-xl font-bold text-gray-900">
    Итого: {{ totalPrice.toFixed(2) }} ₽
  </div>
</div>

    </div>

    <!-- Кнопка подтверждения -->
    <div class="text-right mt-10">
      <el-button
        type="primary"
        size="large"
        :loading="loading"
        @click="submitOrder"
        class="!bg-primary-600 hover:!bg-primary-700 !px-10 !py-3 rounded-lg text-base"
      >
        Перейти к оплате
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
const selectedAddressId = orderStore.selectedAddressId
const selectedAddress = orderStore.selectedAddress
const isDelivery = orderStore.isDelivery
const paymentMethod = orderStore.paymentMethod
const deliveryComment = orderStore.deliveryComment
const cartItems = cartStore.items
const totalPrice = cartStore.totalPrice

const fullPharmacyAddress = computed(() =>
  selectedPharmacy ? selectedPharmacy.address : ''
)

const submitOrder = async () => {
  try {
    loading.value = true

    const payload = {
      paymentMethod,
      isDelivery,
      deliveryComment
    }
    if (isDelivery) {
      payload.userAddressId = selectedAddressId
    } else {
      payload.pharmacyId = selectedPharmacy.id
    }

    const response = await api.post('/orders', payload)

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
.custom-scroll::-webkit-scrollbar {
  width: 6px;
}
.custom-scroll::-webkit-scrollbar-thumb {
  background-color: rgba(0, 0, 0, 0.15);
  border-radius: 8px;
}
.custom-scroll::-webkit-scrollbar-track {
  background-color: transparent;
}
</style>
