<template>
  <div class="relative w-full h-full">
    <!-- Избранное -->
    <button @click="toggleFavorite" class="absolute top-3 right-4 z-20 text-xl transition">
      <i
        class="fas fa-heart"
        :class="isFavorite ? 'text-red-500' : 'text-gray-300 hover:text-red-500 transition'"
      ></i>
    </button>

    <!-- Обёртка карточки -->
    <div
      class="bg-white border rounded-xl shadow-sm hover:shadow-md transition overflow-hidden flex flex-col h-full min-h-[370px]"
      :class="{ 'opacity-60 grayscale': !product.isAvailable }"
    >
      <!-- Изображение -->
      <router-link
        :to="productLink"
        class="relative h-52 bg-gray-100 flex items-center justify-center overflow-hidden"
      >
        <img
          v-if="product.imageUrl"
          :src="product.imageUrl || defaultImage"
          alt="image"
          class="w-full h-full object-contain"
        />
        <i v-else class="fas fa-image text-4xl text-gray-400"></i>

        <!-- Плашки -->
        <div
          v-if="!product.isAvailable"
          class="absolute bottom-2 left-2 bg-gray-300 text-gray-800 text-xs font-medium px-2 py-0.5 rounded"
        >
          Нет в наличии
        </div>
        <div
          v-else-if="product.isPrescriptionRequired"
          class="absolute bottom-2 left-2 bg-blue-100 text-primary-600 text-xs font-medium px-2 py-0.5 rounded flex items-center gap-1"
        >
          <i class="fas fa-prescription-bottle-alt"></i>
          По рецепту
        </div>
      </router-link>

      <!-- Контент -->
      <div class="p-4 flex flex-col flex-grow justify-between relative">
        <div class="mb-2">
          <router-link
            :to="productLink"
            class="text-base font-semibold text-gray-900 hover:underline block line-clamp-2"
          >
            {{ product.name }}
          </router-link>
          <p class="text-sm text-gray-500 mt-1 line-clamp-2">
            {{ product.description || 'Описание недоступно' }}
          </p>
        </div>

        <!-- Цена и кнопки -->
        <div class="pt-2 flex items-center justify-between gap-2 mt-auto">
          <span class="font-semibold text-gray-900 text-lg">
            {{ product.price.toFixed(2) }} ₽
          </span>

          <div class="flex items-center gap-1">
            <div v-show="cartQuantity > 0" class="flex items-center gap-1">
              <button
                @click="decrementQuantity"
                class="w-8 h-8 flex items-center justify-center bg-gray-200 rounded hover:bg-gray-300"
              >
                <i class="fas fa-minus text-xs"></i>
              </button>

              <input
                v-model.number="editableQuantity"
                @change="setQuantity"
                type="number"
                min="1"
                class="w-12 h-8 text-center border rounded focus:outline-none text-sm"
              />

              <button
                @click="incrementQuantity"
                class="w-8 h-8 flex items-center justify-center bg-gray-200 rounded hover:bg-gray-300"
              >
                <i class="fas fa-plus text-xs"></i>
              </button>
            </div>

            <button
              v-show="cartQuantity === 0"
              :disabled="!product.isAvailable"
              @click="addToCart"
              class="bg-primary-600 text-white px-3 py-1.5 rounded text-sm hover:bg-primary-700 transition disabled:opacity-50 disabled:cursor-not-allowed"
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
const productLink = computed(() => `/products/${props.product.id}-${toSlug(props.product.name)}`)

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
