<template>
  <div class="bg-slate-50 rounded-xl shadow hover:shadow-lg transition overflow-hidden relative flex flex-col">
    <!-- Изображение -->
    <div class="relative h-48 bg-gray-100 flex items-center justify-center overflow-hidden">
      <img :src="product.imageUrl || defaultImage" alt="product image" class="w-full h-full object-cover" />

      <!-- Избранное (всегда доступна) -->
      <button @click="toggleFavorite" class="absolute top-2 right-2 text-xl">
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

        <div v-if="cartQuantity > 0" class="flex items-center gap-2">
          <button @click="decrementQuantity" class="px-2 py-1 bg-gray-200 rounded hover:bg-gray-300">−</button>
          <input
            v-model.number="editableQuantity"
            @change="setQuantity"
            type="number"
            min="1"
            class="w-12 text-center border rounded"
          />
          <button @click="incrementQuantity" class="px-2 py-1 bg-gray-200 rounded hover:bg-gray-300">+</button>
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
import { useFavoritesStore } from '/src/store/FavoritesStore'
import { useCartStore } from '/src/store/CartStore'

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

const toggleFavorite = () => {
  favoritesStore.toggle(props.product.id, isFavorite.value)
}

const addToCart = () => {
  cartStore.addToCart(props.product.id)
}

const incrementQuantity = () => {
  cartStore.increment(props.product.id)
}

const decrementQuantity = () => {
  cartStore.decrement(props.product.id)
}

const setQuantity = () => {
  const quantity = Number(editableQuantity.value)
  if (!Number.isInteger(quantity) || quantity < 1) {
    editableQuantity.value = cartQuantity.value || 1
    return
  }
  cartStore.setQuantity(props.product.id, quantity)
}
</script>
