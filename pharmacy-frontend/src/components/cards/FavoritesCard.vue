<template>
  <div class="bg-white border border-gray-200 rounded-xl hover:shadow-md transition relative flex overflow-hidden">
    <!-- Изображение -->
    <div class="relative w-64 h-auto aspect-[4/3] flex-shrink-0 bg-gray-100 flex items-center justify-center overflow-hidden">
      <img
        v-if="product.imageUrl"
        :src="product.imageUrl"
        alt="image"
        class="w-full h-full object-cover"
      />
      <i v-else class="fas fa-image text-5xl text-gray-400"></i>

      <!-- Плашка: только одна -->
      <div
        v-if="!product.isAvailable"
        class="absolute bottom-2 left-2 bg-gray-300 text-gray-800 text-xs font-medium px-2 py-0.5 rounded"
      >
        Нет в наличии
      </div>
      <div
        v-else-if="product.isPrescriptionRequired"
        class="absolute bottom-2 left-2 bg-blue-100 text-primary-600 text-xs font-medium px-2 py-0.5 rounded inline-flex items-center gap-1"
      >
        <i class="fas fa-file-prescription"></i>
        По рецепту
      </div>
    </div>

    <!-- Контент -->
    <div class="p-6 flex flex-col flex-grow justify-between relative w-full">
      <!-- Значок избранного -->
      <button @click="toggleFavorite" class="absolute top-4 right-6 text-3xl">
        <i :class="['fas fa-heart', isFavorite ? 'text-red-500' : 'text-gray-300 hover:text-red-500']"></i>
      </button>

      <div class="pr-10">
        <h3 class="text-xl font-semibold text-gray-900 mb-1">{{ product.name }}</h3>
        <p class="text-base text-gray-600 line-clamp-2">{{ product.description || 'Описание недоступно' }}</p>
        <p class="text-base text-gray-500 mb-1">{{ product.manufacturerName }} ({{ product.manufacturerCountry }})</p>
      </div>

      <div class="mt-6 flex justify-between items-center">
        <span class="text-lg font-bold text-gray-900">{{ product.price.toFixed(2) }} ₽</span>

        <template v-if="cartQuantity > 0">
          <div class="flex items-center gap-2">
            <button class="h-8 px-3 border rounded" @click="decrementQuantity">-</button>
            <input
              v-model.number="editableQuantity"
              @change="setQuantity"
              type="number"
              min="1"
              class="w-12 h-8 text-center border rounded appearance-none focus:outline-none"
            />

            <button class="h-8 px-3 border rounded" @click="incrementQuantity">+</button>
          </div>
        </template>
        <template v-else>
          <button
            class="bg-primary-600 hover:bg-primary-700 text-white px-5 py-2 rounded"
            :disabled="!product.isAvailable"
            @click="addToCart"
          >
            В корзину
          </button>
        </template>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import { useFavoritesStore } from '/src/store/FavoritesStore'
import { useCartStore } from '/src/store/CartStore'

const props = defineProps({
  product: Object
})

const favoritesStore = useFavoritesStore()
const cartStore = useCartStore()

const isFavorite = computed(() => favoritesStore.ids.includes(props.product.productId))
const cartQuantity = computed(() => cartStore.quantityById[props.product.productId] || 0)
const editableQuantity = ref(cartQuantity.value || 1)

watch(cartQuantity, val => {
  editableQuantity.value = val || 1
})

function toggleFavorite() {
  favoritesStore.toggle(props.product.productId, isFavorite.value)
}

function addToCart() {
  cartStore.addToCart(props.product.productId)
}

function incrementQuantity() {
  cartStore.increment(props.product.productId)
}

function decrementQuantity() {
  cartStore.decrement(props.product.productId)
}

function setQuantity() {
  const quantity = Number(editableQuantity.value)
  if (!Number.isInteger(quantity) || quantity < 1) {
    editableQuantity.value = cartQuantity.value || 1
    return
  }
  cartStore.setQuantity(props.product.productId, quantity)
}
</script>

<style scoped>
.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
input[type="number"]::-webkit-inner-spin-button,
input[type="number"]::-webkit-outer-spin-button {
  -webkit-appearance: none;
  margin: 0;
}
input[type="number"] {
  -moz-appearance: textfield;
}
</style>