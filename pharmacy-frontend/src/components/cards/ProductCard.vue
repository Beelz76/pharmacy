<template>
  <div class="relative w-full">
    <!-- Избранное -->
    <button @click="toggleFavorite" class="absolute top-2 right-4 text-2xl z-20">
      <i :class="['fas fa-heart', isFavorite ? 'text-red-500' : 'text-gray-300 hover:text-red-500']"></i>
    </button>

    <!-- Обёртка карточки -->
    <div
      class="bg-white rounded-xl shadow hover:shadow-lg transition overflow-hidden flex flex-col"
      :class="{ 'opacity-60 grayscale': !product.isAvailable }"
    >
      <!-- Изображение -->
      <router-link
        :to="productLink"
        class="relative h-48 bg-gray-100 flex items-center justify-center overflow-hidden"
      >
        <img
          v-if="product.imageUrl"
          :src="product.imageUrl || defaultImage"
          alt="image"
          class="w-full h-full object-contain"
        />
        <i v-else class="fas fa-image text-3xl text-gray-400"></i>

        <!-- Плашки -->
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
      </router-link>

      <!-- Контент -->
      <div class="p-4 flex flex-col flex-grow justify-between relative">
        <div>
          <router-link
            :to="productLink"
            class="text-base font-semibold text-gray-900 hover:underline"
          >
            {{ product.name }}
          </router-link>
          <p class="text-sm text-gray-500 mt-1">
            {{ product.description || 'Описание недоступно' }}
          </p>
        </div>

<div class="mt-4 min-h-[42px] flex items-center justify-between gap-2">
  <span class="font-semibold text-gray-900">{{ product.price.toFixed(2) }} ₽</span>

  <div class="flex items-center gap-1">
    <!-- Контролы количества, если товар уже в корзине -->
    <div v-show="cartQuantity > 0" class="flex items-center gap-1">
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

    <!-- Кнопка "В корзину", если товара ещё нет -->
    <button
      v-show="cartQuantity === 0"
      :disabled="!product.isAvailable"
      @click="addToCart"
      class="bg-primary-600 text-white px-3 py-1 rounded text-sm hover:bg-primary-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
    >
      В корзину
    </button>
  </div>
</div>

      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, watch, computed } from 'vue'
import { useFavoritesStore } from '/src/stores/FavoritesStore'
import { useCartStore } from '/src/stores/CartStore'
import { toSlug } from '/src/utils/slugify'

const props = defineProps({ product: Object })
const defaultImage = 'https://via.placeholder.com/300x200?text=No+Image'

const favoritesStore = useFavoritesStore()
const cartStore = useCartStore()

const isFavorite = computed(() => favoritesStore.ids.includes(props.product.id))
const cartQuantity = computed(() => cartStore.quantityById[props.product.id] || 0)
const productLink = computed(() => `/products/${toSlug(props.product.name)}`)

const editableQuantity = computed({
  get: () => cartQuantity.value || 1,
  set: async (val) => {
    const quantity = Number(val)
    if (!Number.isInteger(quantity) || quantity < 1) return
    try {
      await cartStore.setQuantity(props.product.id, quantity)
    } catch (err) {
      console.error('Ошибка при изменении количества:', err)
    }
  }
})

const toggleFavorite = async () => {
  try {
    await favoritesStore.toggle(props.product.id, isFavorite.value)
  } catch (err) {
    console.error('Ошибка при обновлении избранного:', err)
  }
}

const addToCart = async () => {
  try {
    await cartStore.addToCart(props.product.id)
  } catch (err) {
    console.error('Ошибка при добавлении в корзину:', err)
  }
}

const incrementQuantity = async () => {
  try {
    await cartStore.increment(props.product.id)
  } catch (err) {
    console.error('Ошибка при увеличении количества:', err)
  }
}

const decrementQuantity = async () => {
  try {
    await cartStore.decrement(props.product.id)
  } catch (err) {
    console.error('Ошибка при уменьшении количества:', err)
  }
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
