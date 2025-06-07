<template>
  <div class="relative w-full min-h-[160px]">
    <!-- Иконка избранного -->
    <button
      @click.stop="toggleFavorite"
      class="absolute top-3 left-3 text-[18px] z-20 transition"
    >
      <i
        class="fas fa-heart"
        :class="isFavorite ? 'text-red-500' : 'text-gray-300 hover:text-red-500 transition'"
      ></i>
    </button>

    <!-- Карточка -->
    <div
      class="bg-white border border-gray-200 rounded-xl hover:shadow-md transition flex overflow-hidden w-full h-full"
      :class="{ 'opacity-60 grayscale': !product.isAvailable }"
    >
      <!-- Изображение -->
      <router-link
        :to="productLink"
        class="relative w-44 h-44 flex items-center justify-center bg-gray-100 flex-shrink-0 overflow-hidden"
      >
        <img
          v-if="product.imageUrl"
          :src="product.imageUrl"
          alt="product image"
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
          class="absolute bottom-2 left-2 bg-blue-100 text-primary-600 text-xs font-medium px-2 py-0.5 rounded inline-flex items-center gap-1"
        >
          <i class="fas fa-file-prescription"></i>
          По рецепту
        </div>
      </router-link>

      <!-- Контент -->
      <div class="p-5 flex flex-col justify-between flex-grow">
        <!-- Название, описание -->
        <div>
          <router-link
            :to="productLink"
            class="text-base font-semibold text-gray-900 hover:underline"
          >
            {{ product.name }}
          </router-link>
          <p class="text-sm text-gray-500 mt-1 line-clamp-2">
            {{ product.description || 'Описание недоступно' }}
          </p>
        </div>

        <!-- Цена и управление -->
        <div class="mt-4 flex items-center justify-between gap-2 min-h-[42px]">
          <span class="text-base font-semibold text-gray-900 whitespace-nowrap">
            {{ product.price.toFixed(2) }} ₽
          </span>

          <div class="flex items-center gap-1">
            <!-- Контролы количества -->
            <div v-show="cartQuantity > 0" class="flex items-center gap-1">
              <button
                @click="decrementQuantity"
                class="w-8 h-8 flex items-center justify-center bg-gray-200 rounded hover:bg-gray-300 text-sm"
              >
                <i class="fas fa-minus text-xs"></i>
              </button>

              <input
                v-model.number="editableQuantity"
                @change="setQuantity"
                type="number"
                min="1"
                class="w-10 h-8 text-center border rounded text-sm focus:outline-none"
              />

              <button
                @click="incrementQuantity"
                class="w-8 h-8 flex items-center justify-center bg-gray-200 rounded hover:bg-gray-300 text-sm"
              >
                <i class="fas fa-plus text-xs"></i>
              </button>
            </div>

            <!-- Кнопка добавления -->
            <button
              v-show="cartQuantity === 0"
              :disabled="!product.isAvailable"
              @click="addToCart"
              class="bg-primary-600 text-white px-3 py-1.5 rounded text-sm hover:bg-primary-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
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
import { ref, computed, watch } from 'vue'
import { useFavoritesStore } from '/src/stores/FavoritesStore'
import { useCartStore } from '/src/stores/CartStore'
import { toSlug } from '/src/utils/slugify'

const props = defineProps({ product: Object })

const favoritesStore = useFavoritesStore()
const cartStore = useCartStore()
const productId = props.product.productId

const isFavorite = computed(() =>
  favoritesStore.ids.includes(productId)
)

const cartQuantity = computed(() =>
  cartStore.quantityById[productId] || 0
)
const productLink = computed(() => `/products/${productId}-${toSlug(props.product.name)}`)

const editableQuantity = computed({
  get: () => cartQuantity.value || 1,
  set: async (val) => {
    const quantity = Number(val)
    if (!Number.isInteger(quantity) || quantity < 1) return
    try {
      await cartStore.setQuantity(productId, quantity)
    } catch (err) {
      console.error('Ошибка при изменении количества:', err)
    }
  }
})

const toggleFavorite = async () => {
  try {
    await favoritesStore.toggle(productId, isFavorite.value)
  } catch (err) {
    console.error('Ошибка при обновлении избранного:', err)
  }
}

const addToCart = async () => {
  try {
    await cartStore.addToCart(productId, props.product)
  } catch (err) {
    console.error('Ошибка при добавлении в корзину:', err)
  }
}

const incrementQuantity = async () => {
  try {
    await cartStore.increment(productId, props.product)
  } catch (err) {
    console.error('Ошибка при увеличении количества:', err)
  }
}

const decrementQuantity = async () => {
  try {
    await cartStore.decrement(productId)
  } catch (err) {
    console.error('Ошибка при уменьшении количества:', err)
  }
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
