<template>
  <div class="bg-white border border-gray-200 rounded-xl hover:shadow-md transition relative flex overflow-hidden">
    <!-- Изображение -->
    <div class="relative w-48 h-auto aspect-[4/3] flex-shrink-0 bg-gray-100 flex items-center justify-center">
      <img
        v-if="product.imageUrl"
        :src="product.imageUrl"
        alt="image"
        class="w-full h-full object-cover"
      />
      <i v-else class="fas fa-image text-4xl text-gray-400"></i>

      <!-- Плашка: Нет в наличии -->
      <div
        v-if="!product.isAvailable"
        class="absolute bottom-2 left-2 bg-gray-300 text-gray-800 text-xs font-medium px-2 py-0.5 rounded"
      >
        Нет в наличии
      </div>

      <!-- Плашка: По рецепту -->
      <div
        v-else-if="product.isPrescriptionRequired"
        class="absolute bottom-2 left-2 bg-blue-100 text-primary-600 text-xs font-medium px-2 py-0.5 rounded inline-flex items-center gap-1"
      >
        <i class="fas fa-file-prescription"></i>
        По рецепту
      </div>
    </div>

<!-- Контент -->
<div class="flex flex-col flex-grow justify-between min-h-44 p-4 w-full relative">
  <!-- Кнопка Удалить -->
  <button @click="removeItem" class="absolute top-2 right-4 text-gray-400 hover:text-red-500 text-xl">
    <i class="fas fa-times"></i>
  </button>

  <div class="mb-2">
    <h3 class="text-lg font-semibold text-gray-900 leading-snug">{{ product.name }}</h3>
    <p class="mt-3 text-base text-gray-500">{{ product.manufacturerName }} ({{ product.manufacturerCountry }})</p>
  </div>

  <!-- Цена за штуку -->
  <div class="text-sm text-gray-700">
    <span class="font-medium">{{ product.unitPrice.toFixed(2) }} ₽ / шт.</span>
  </div>

  <div class="flex items-center justify-between">
    <!-- Кол-во -->
    <div class="flex items-center gap-2">
      <button
        class="w-8 h-8 flex items-center justify-center bg-gray-200 rounded hover:bg-gray-300"
        @click="decrementQuantity"
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
        class="w-8 h-8 flex items-center justify-center bg-gray-200 rounded hover:bg-gray-300"
        @click="incrementQuantity"
      >
        +
      </button>
    </div>

    <!-- Общая цена -->
    <span class="text-lg font-bold text-gray-900">{{ product.totalPrice.toFixed(2) }} ₽</span>
  </div>
</div>

  </div>
</template>


<script setup>
import { ref, computed, watch } from 'vue'
import { useFavoritesStore } from '/src/stores/FavoritesStore'
import { useCartStore } from '/src/stores/CartStore'

const props = defineProps({ product: Object })

const favoritesStore = useFavoritesStore()
const cartStore = useCartStore()

const isFavorite = computed(() => favoritesStore.ids.includes(props.product.productId))
const cartQuantity = computed(() => cartStore.quantityById[props.product.productId] || 0)
const editableQuantity = ref(cartQuantity.value || 1)

watch(cartQuantity, val => {
  editableQuantity.value = val || 1
})

const toggleFavorite = () => {
  favoritesStore.toggle(props.product.productId, isFavorite.value)
}

const incrementQuantity = () => {
  cartStore.increment(props.product.productId)
}

const decrementQuantity = () => {
  cartStore.decrement(props.product.productId)
}

const setQuantity = () => {
  const quantity = Number(editableQuantity.value)
  if (!Number.isInteger(quantity) || quantity < 1) {
    editableQuantity.value = cartQuantity.value || 1
    return
  }
  cartStore.setQuantity(props.product.productId, quantity)
}

const removeItem = () => {
  cartStore.removeItem(props.product.productId)
}
</script>

<style scoped>
input[type="number"]::-webkit-inner-spin-button,
input[type="number"]::-webkit-outer-spin-button {
  -webkit-appearance: none;
  margin: 0;
}
input[type="number"] {
  -moz-appearance: textfield;
}
</style>
