<template>
  <div class="max-w-4xl mx-auto py-8 px-4">
    <!-- Заголовок -->
    <div class="flex items-center gap-3 mb-8">
      <router-link
        :to="{ name: 'Checkout' }"
        class="flex items-center text-primary-600 hover:text-primary-700 text-xl group"
      >
        <i class="fas fa-arrow-left mr-2 group-hover:-translate-x-1 transition-transform duration-150"></i>
      </router-link>
      <h2 class="text-2xl font-bold">Подтверждение заказа</h2>
    </div>

    <!-- Адрес и оплата -->
    <div class="mb-6 space-y-4 text-base text-gray-700">
      <div v-if="selectedPharmacy">
        <span class="font-medium">Самовывоз из аптеки:</span><br />
        {{ fullPharmacyAddress }}
      </div>
      <div v-if="paymentMethod">
        <span class="font-medium">Способ оплаты:</span><br />
        {{ paymentMethod === 'Online' ? 'Картой онлайн' : 'При получении' }}
      </div>
    </div>

    <!-- Состав заказа -->
    <div class="bg-gray-50 border border-gray-200 rounded-xl p-6 mb-6 shadow-sm">
      <h3 class="text-lg font-semibold mb-4 border-b border-gray-200 pb-2">Состав заказа</h3>

      <div
        v-for="item in cartItems"
        :key="item.productId"
        class="flex justify-between items-center py-3 border-b last:border-b-0"
      >
        <div class="flex-1">
          <div class="font-medium text-gray-900">{{ item.name }}</div>
          <div class="text-sm text-gray-500">{{ item.description }}</div>
        </div>
        <div class="flex items-center gap-4">
          <span class="text-gray-500 text-sm">×{{ item.quantity }}</span>
          <div class="font-semibold">{{ item.totalPrice.toFixed(2) }} ₽</div>
        </div>
      </div>

      <div class="text-right mt-6 text-lg font-bold">
        Итого: {{ totalPrice.toFixed(2) }} ₽
      </div>
    </div>

    <!-- Кнопка -->
    <div class="text-right">
      <el-button
        type="primary"
        size="large"
        :loading="loading"
        @click="submitOrder"
      >
        Подтвердить заказ
      </el-button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useCartStore } from '../stores/CartStore'
import { useOrderStore } from '../stores/OrderStore'
import api from '../utils/axios'

const router = useRouter()
const cartStore = useCartStore()
const orderStore = useOrderStore()

const loading = ref(false)

const selectedPharmacy = orderStore.selectedPharmacy
const paymentMethod = orderStore.paymentMethod
const cartItems = cartStore.items
const totalPrice = cartStore.totalPrice

const fullPharmacyAddress = computed(() =>
  selectedPharmacy ? `${selectedPharmacy.address}, ${selectedPharmacy.name}` : ''
)

onMounted(() => {
  if (!selectedPharmacy || !paymentMethod) {
    router.replace({ name: 'Checkout' })
  }
})

const submitOrder = async () => {
  try {
    loading.value = true

    await api.post('/orders', {
      pharmacyAddress: fullPharmacyAddress.value,
      paymentMethod
    })

    cartStore.resetCart()
    orderStore.resetOrder()

    router.push({ name: 'OrderSuccess' })
  } catch (error) {
    console.error('Ошибка при оформлении заказа:', error)
  } finally {
    loading.value = false
  }
}
</script>
