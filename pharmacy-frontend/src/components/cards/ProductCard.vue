<template>
  <div class="bg-white rounded-xl shadow hover:shadow-lg transition overflow-hidden relative flex flex-col">
    <!-- Изображение -->
    <div class="relative h-48 bg-gray-100 flex items-center justify-center overflow-hidden">
      <img v-if="product.imageUrl" :src="product.imageUrl || defaultImage" alt="image" class="w-full h-full object-cover" />
        <i v-else class="fas fa-image text-3xl text-gray-400"></i>

      <!-- Избранное (всегда доступна) -->
      <button @click="toggleFavorite" class="absolute top-2 right-4 text-2xl">
        <i :class="['fas fa-heart', isFavorite ? 'text-red-500' : 'text-gray-300 hover:text-red-500']"></i>
      </button>

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
    <div class="p-4 flex flex-col flex-grow justify-between relative">
      <div>
        <h3 class="text-base font-semibold text-gray-900">{{ product.name }}</h3>
        <p class="text-sm text-gray-500 mt-1">{{ product.description || 'Описание недоступно' }}</p>
      </div>

      <div class="mt-4 flex items-center justify-between flex-wrap gap-2">
        <span class="font-semibold text-gray-900">{{ product.price.toFixed(2) }} ₽</span>

<div v-if="cartQuantity > 0" class="flex items-center gap-1">
  <button
    @click="decrementQuantity"
    class="w-8 h-8 flex items-center justify-center bg-gray-200 rounded hover:bg-gray-300"
  >
    −
  </button>

  <input
    v-model.number="editableQuantity"
    @change="setQuantity"
    type="number"
    min="1"
    class="w-12 h-8 text-center border rounded appearance-none focus:outline-none"
  />

  <button
    @click="incrementQuantity"
    class="w-8 h-8 flex items-center justify-center bg-gray-200 rounded hover:bg-gray-300"
  >
    +
  </button>
</div>

        <button
          v-else
          :disabled="!product.isAvailable"
          @click="addToCart"
          class="bg-primary-600 text-white px-3 py-1 rounded text-sm hover:bg-primary-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
        >
          В корзину
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, watch, computed } from 'vue'
import { useFavoritesStore } from '/src/stores/FavoritesStore'
import { useCartStore } from '/src/stores/CartStore'

const props = defineProps({ product: Object })
const defaultImage = 'https://via.placeholder.com/300x200?text=No+Image'

const favoritesStore = useFavoritesStore()
const cartStore = useCartStore()

const isFavorite = computed(() => favoritesStore.ids.includes(props.product.id))
const cartQuantity = computed(() => cartStore.quantityById[props.product.id] || 0)

const editableQuantity = ref(cartQuantity.value || 1)

watch(cartQuantity, val => {
  editableQuantity.value = val || 1
})

const toggleFavorite = async () => {
  await favoritesStore.toggle(props.product.id, isFavorite.value)
}

const addToCart = async() => {
  await cartStore.addToCart(props.product.id)
}

const incrementQuantity = async () => {
  await cartStore.increment(props.product.id)
}

const decrementQuantity = async() => {
  await cartStore.decrement(props.product.id)
}

const setQuantity = async() => {
  const quantity = Number(editableQuantity.value)
  if (!Number.isInteger(quantity) || quantity < 1) {
    editableQuantity.value = cartQuantity.value || 1
    return
  }
  await cartStore.setQuantity(props.product.id, quantity)
}
</script>

<style scoped>
/* Удаляем стрелки у number input */
input[type="number"]::-webkit-inner-spin-button,
input[type="number"]::-webkit-outer-spin-button {
  -webkit-appearance: none;
  margin: 0;
}
input[type="number"] {
  -moz-appearance: textfield;
}
</style>