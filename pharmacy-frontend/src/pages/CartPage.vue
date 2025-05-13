<template>
  <div class="max-w-7xl mx-auto py-8 px-2">
    <div class="flex items-center gap-3 mb-6">
      <h2 class="text-2xl font-bold">Корзина</h2>
    </div>

    <div v-if="items.length" class="flex flex-col lg:flex-row lg:items-start gap-8">
      <!-- Список товаров -->
      <div class="flex-1 space-y-4">
        <CartCard
          v-for="item in paginatedItems"
          :key="item.productId"
          :product="item"
        />

        <!-- Кнопка Очистить и Пагинация -->
        <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between mt-4 gap-4">
          <el-button
            @click="cart.clearCart"
            type="plain"
            class="text-sm text-blue-600 w-fit"
          >
            Очистить
          </el-button>

          <div v-if="totalPages > 1" class="flex justify-center sm:justify-center w-full">
            <el-pagination
              layout="prev, pager, next"
              :total="items.length"
              :page-size="pageSize"
              v-model:current-page="currentPage"
              class="!text-xl scale-110"
            />
          </div>
        </div>
      </div>

      <!-- Информация о заказе -->
      <div class="w-full lg:w-80 bg-gray-50 border rounded-xl p-6 shadow-sm space-y-4 self-start sticky top-24">
        <h3 class="text-lg font-semibold">Ваша корзина</h3>
        <div class="flex justify-between text-sm">
          <span>Товары</span>
          <span>{{ totalCount }} шт.</span>
        </div>
        <div class="flex justify-between font-semibold text-base border-t pt-2">
          <span>Итого</span>
          <span>{{ totalPrice.toFixed(2) }} ₽</span>
        </div>

<router-link to="/cart/checkout" class="block">
  <el-button
    type="primary"
    size="large"
    class="w-full !bg-primary-600 hover:!bg-primary-700"
    :disabled="!allAvailable"
  >
    Перейти к оформлению
  </el-button>
</router-link>

      </div>
    </div>

    <div v-else class="text-center text-gray-500 py-20">
      <p class="text-lg">Ваша корзина пуста</p>
      <router-link to="/products" class="text-primary-600 hover:underline">Перейти к покупкам</router-link>
    </div>
  </div>
</template>

<script setup>
import { onMounted, computed, ref } from 'vue'
import { storeToRefs } from 'pinia'
import { useCartStore } from '/src/stores/CartStore'
import CartCard from '/src/components/cards/CartCard.vue'

const cart = useCartStore()
const { items, totalPrice } = storeToRefs(cart)

const currentPage = ref(1)
const pageSize = 4

onMounted(() => {
  cart.fetchCart()
})

const allAvailable = computed(() => items.value.every(p => p.isAvailable))
const totalCount = computed(() => items.value.reduce((sum, p) => sum + p.quantity, 0))

const totalPages = computed(() => Math.ceil(items.value.length / pageSize))
const paginatedItems = computed(() => {
  const start = (currentPage.value - 1) * pageSize
  return items.value.slice(start, start + pageSize)
})
</script>
