<template>
  <section class="py-12 bg-gray-50">
    <div class="max-w-7xl mx-auto px-4">
      <!-- Заголовок -->
      <div class="text-center mb-8">
        <p class="text-primary-600 font-semibold uppercase">Последние товары</p>
        <h2 class="text-3xl font-extrabold text-gray-900 mt-2">Новинки</h2>
      </div>

      <!-- Слайдер -->
      <div v-if="products.length" class="relative">
        <!-- Стрелка влево -->
        <button
          @click="prevSlide"
          v-if="canScrollLeft"
          class="absolute left-0 top-1/2 -translate-y-1/2 z-10 bg-white border rounded-full shadow p-2 hover:bg-gray-100"
        >
          <i class="fas fa-chevron-left"></i>
        </button>

        <div class="overflow-hidden">
          <div
            class="flex transition-transform duration-500 ease-in-out"
            :style="{ transform: `translateX(-${currentSlide * 100}%)` }"
          >
            <div
              v-for="product in products"
              :key="product.id"
              class="w-full sm:w-1/2 md:w-1/3 lg:w-1/4 flex-shrink-0 p-2"
            >
              <ProductCard
                :product="product"
                @add-to-cart="onAddToCart"
                @increment="onIncrement"
                @decrement="onDecrement"
                @toggle-favorite="onToggleFavorite"
              />
            </div>
          </div>
        </div>

        <!-- Стрелка вправо -->
        <button
          @click="nextSlide"
          v-if="canScrollRight"
          class="absolute right-0 top-1/2 -translate-y-1/2 z-10 bg-white border rounded-full shadow p-2 hover:bg-gray-100"
        >
          <i class="fas fa-chevron-right"></i>
        </button>
      </div>

      <!-- Пустой список -->
      <div v-else class="text-center text-gray-500 mt-6">Загрузка товаров...</div>
    </div>
  </section>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import api from '../utils/axios'
import ProductCard from './ProductCard.vue'

const products = ref([])
const currentSlide = ref(0)
const itemsPerPage = 4
const maxVisibleItems = 12

const canScrollLeft = computed(() => currentSlide.value > 0)
const canScrollRight = computed(() => (currentSlide.value + 1) * itemsPerPage < products.value.length)

onMounted(async () => {
  try {
    const response = await api.post('/products/paginated', {
      params: {
        pageNumber: 1,
        pageSize: maxVisibleItems
      }
    })
    products.value = response.data.items
  } catch (err) {
    console.error('Ошибка загрузки товаров:', err)
  }
})

function nextSlide() {
  if (canScrollRight.value) currentSlide.value++
}

function prevSlide() {
  if (canScrollLeft.value) currentSlide.value--
}

function onAddToCart(productId) {
  console.log('Добавить в корзину:', productId)
}

function onIncrement(productId) {
  console.log('Увеличить количество:', productId)
}

function onDecrement(productId) {
  console.log('Уменьшить количество:', productId)
}

function onToggleFavorite(productId) {
  console.log('Избранное переключено:', productId)
}
</script>
