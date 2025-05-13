<template>
  <div class="bg-white border border-gray-200 rounded-xl hover:shadow-md transition flex relative overflow-hidden w-full min-h-[160px]">
    <!-- Иконка избранного -->
    <button
      @click.stop="toggleFavorite"
      class="absolute top-2 left-2 text-lg text-primary-600 hover:text-primary-700 z-10"
    >
      <i :class="['fas fa-heart', isFavorite ? 'text-red-500' : 'text-gray-300 hover:text-red-500']"></i>
    </button>

    <!-- Изображение -->
    <div class="relative w-40 h-full flex items-center justify-center bg-gray-100 flex-shrink-0 overflow-hidden">
      <img
        v-if="product.imageUrl"
        :src="product.imageUrl"
        alt="product image"
        class="w-full h-full object-cover"
      />
      <i v-else class="fas fa-image text-3xl text-gray-400"></i>

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
    <div class="p-5 flex flex-col justify-between flex-grow">
      <!-- Название и описание -->
      <div>
        <h3 class="text-lg font-semibold text-gray-900">{{ product.name }}</h3>
        <p class="text-sm text-gray-500 mt-1 line-clamp-2">
          {{ product.description || 'Описание недоступно' }}
        </p>
        <p class="text-xs text-gray-400 mt-1">
          {{ product.manufacturerName }} ({{ product.manufacturerCountry }})
        </p>
      </div>

      <!-- Цена и корзина -->
      <div class="mt-4 flex items-center justify-between flex-wrap gap-2">
        <span class="text-base font-semibold text-gray-900">{{ product.price.toFixed(2) }} ₽</span>

        <template v-if="cartQuantity > 0">
          <div class="flex items-center gap-1">
            <button
              @click="decrementQuantity"
              class="w-8 h-8 flex items-center justify-center bg-gray-200 rounded hover:bg-gray-300 text-sm"
            >
              −
            </button>

            <input
              v-model.number="editableQuantity"
              @change="setQuantity"
              type="number"
              min="1"
              class="w-10 h-8 text-center border rounded text-sm"
            />

            <button
              @click="incrementQuantity"
              class="w-8 h-8 flex items-center justify-center bg-gray-200 rounded hover:bg-gray-300 text-sm"
            >
              +
            </button>
          </div>
        </template>

        <el-button
          v-else
          size="small"
          :disabled="!product.isAvailable"
          @click="addToCart"
          class="!bg-primary-600 hover:!bg-primary-700 text-white !px-4 !py-1.5 text-sm rounded disabled:opacity-50"
        >
          В корзину
        </el-button>
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

const isFavorite = computed(() =>
  favoritesStore.ids.includes(props.product.productId)
)

const cartQuantity = computed(() =>
  cartStore.quantityById[props.product.productId] || 0
)

const editableQuantity = ref(cartQuantity.value || 1)

watch(cartQuantity, val => {
  editableQuantity.value = val || 1
})

const toggleFavorite = async () => {
  await favoritesStore.toggle(props.product.productId, isFavorite.value)
}

const addToCart = async () => {
  await cartStore.addToCart(props.product.productId)
}

const incrementQuantity = async () => {
  await cartStore.increment(props.product.productId)
}

const decrementQuantity = async () => {
  await cartStore.decrement(props.product.productId)
}

const setQuantity = async () => {
  const quantity = Number(editableQuantity.value)
  if (!Number.isInteger(quantity) || quantity < 1) {
    editableQuantity.value = cartQuantity.value || 1
    return
  }
  await cartStore.setQuantity(props.product.productId, quantity)
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
