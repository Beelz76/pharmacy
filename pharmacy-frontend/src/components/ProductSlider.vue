<template>
  <section class="py-12 relative bg-gray-50">
    <div class="max-w-7xl mx-auto px-2">
      <div class="text-center mb-8">
        <p class="text-primary-600 font-semibold uppercase">Последние товары</p>
        <h2 class="text-3xl font-extrabold text-gray-900 mt-2">Новинки</h2>
      </div>

      <div v-if="renderedSlides.length" class="relative">
        <!-- Контейнер слайдера с отступом от стрелок -->
        <div class="overflow-hidden">
          <div
            class="flex transition-transform duration-500 ease-in-out"
            :style="{ transform: `translateX(-${slideOffset}px)` }"
            ref="sliderRef"
          >
            <div
              v-for="(product, index) in renderedSlides"
              :key="index"
              class="w-full sm:w-1/2 md:w-1/3 lg:w-1/4 flex-shrink-0 p-2"
              :style="{ minWidth: `${cardWidth}px` }"
            >
              <ProductCard :product="product" />
            </div>
          </div>
        </div>

        <!-- Стрелка влево -->
        <button
          @click="prevSlide"
          class="absolute left-[-2.5rem] top-0 h-full w-10 flex items-center justify-center z-10 bg-transparent focus:outline-none"
        >
          <div class="bg-gray-50 border rounded-full shadow p-2 hover:bg-gray-100">
            <i class="fas fa-chevron-left"></i>
          </div>
        </button>

        <!-- Стрелка вправо -->
        <button
          @click="nextSlide"
          class="absolute right-[-2.5rem] top-0 h-full w-10 flex items-center justify-center z-10 bg-transparent focus:outline-none"
        >
          <div class="bg-gray-50 border rounded-full shadow p-2 hover:bg-gray-100">
            <i class="fas fa-chevron-right"></i>
          </div>
        </button>

      </div>

      <div v-else class="flex justify-center mt-10">
        <LoadingSpinner size="lg" color="primary" class="mx-auto mt-6" />
      </div>
    </div>
  </section>
</template>


<script setup>
import { ref, computed, onMounted, nextTick, watch } from 'vue'
import ProductCard from './cards/ProductCard.vue'
import { getPaginatedProducts } from '../composables/useProducts'
import { useCartStore } from '../stores/CartStore'
import { useFavoritesStore } from '../stores/FavoritesStore'
import { useAuthStore } from '../stores/AuthStore'
import LoadingSpinner from './LoadingSpinner.vue'

const visibleCount = 4
const cardWidth = 300
const products = ref([])
const currentIndex = ref(0)
const sliderRef = ref(null)

const auth = useAuthStore()
const cartStore = useCartStore()
const favoritesStore = useFavoritesStore()

onMounted(async () => {
  const data = await getPaginatedProducts({ page: 1, size: 12 })
  products.value = data.items
  cartStore.syncFromProducts(products.value)
  favoritesStore.syncFromProducts(products.value)
  await nextTick()
})

watch(
  () => auth.isAuthenticated,
  async (val) => {
    if (val) {
      await cartStore.fetchCart()
      await favoritesStore.fetchFavorites()
      applyFavoritesAndCart(products.value)
    }
  }
)

const renderedSlides = computed(() => {
  const extended = [...products.value, ...products.value, ...products.value]
  const start = products.value.length + currentIndex.value
  return extended.slice(start, start + visibleCount)
})

const slideOffset = computed(() => 0)

function applyFavoritesAndCart(products) {
  const ids = favoritesStore.ids
  const quantityById = cartStore.quantityById

  for (const p of products) {
    p.isFavorite = ids.includes(p.id)
    p.cartQuantity = quantityById[p.id] || 0
  }
}

function nextSlide() {
  currentIndex.value = (currentIndex.value + 1) % products.value.length
}

function prevSlide() {
  currentIndex.value =
    (currentIndex.value - 1 + products.value.length) % products.value.length
}
</script>
