<template>
  <div
    class="bg-white border border-gray-200 rounded-xl hover:shadow-lg transition-all flex overflow-hidden relative group w-full min-h-[160px]"
    :class="{ 'opacity-60 grayscale': !product.isAvailable }"
  >
    <!-- Изображение с переходом -->
    <router-link
      :to="productLink"
      class="relative w-40 aspect-[4/3] flex-shrink-0 bg-gray-100 flex items-center justify-center overflow-hidden"
    >
      <img
        v-if="product.imageUrl"
        :src="product.imageUrl"
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
        class="absolute bottom-2 left-2 bg-blue-100 text-primary-600 text-xs font-medium px-2 py-0.5 rounded inline-flex items-center gap-1"
      >
        <i class="fas fa-file-prescription"></i>
        По рецепту
      </div>
    </router-link>

    <!-- Контент -->
    <div class="flex flex-col justify-between flex-grow p-4 pr-12 relative">
      <!-- Удалить -->
      <button
        @click="removeItem"
        class="absolute top-3 right-3 text-gray-400 hover:text-red-500 transition"
        title="Удалить из корзины"
      >
        <i class="fas fa-times text-lg"></i>
      </button>

      <!-- Название и производитель -->
      <div class="mb-2">
        <router-link
          :to="productLink"
          class="text-lg font-semibold text-gray-900 leading-tight line-clamp-2 hover:underline"
        >
          {{ product.name }}
        </router-link>
        <p class="text-sm text-gray-500">
          {{ product.manufacturerName }} ({{ product.manufacturerCountry }})
        </p>
      </div>

      <!-- Цена и управление -->
      <div class="flex items-end justify-between mt-auto">
        <!-- Кол-во -->
        <div class="flex items-center gap-2">
          <button
            class="w-8 h-8 bg-gray-200 rounded hover:bg-gray-300 transition"
            @click="decrementQuantity"
            :disabled="editableQuantity <= 1"
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
            class="w-8 h-8 bg-gray-200 rounded hover:bg-gray-300 transition"
            @click="incrementQuantity"
          >
            +
          </button>
        </div>

        <!-- Общая цена -->
        <div class="text-right ml-4">
          <span class="text-lg font-bold text-gray-900 block">{{ product.totalPrice.toFixed(2) }} ₽</span>
          <span class="text-xs text-gray-500">{{ product.unitPrice.toFixed(2) }} ₽ / шт.</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { useCartStore } from '/src/stores/CartStore'
import { toSlug } from '/src/utils/slugify'

const props = defineProps({ product: Object })

const cartStore = useCartStore()
const productId = props.product.productId

const cartQuantity = computed(() => cartStore.quantityById[productId] || 0)
const productLink = computed(() => `/products/${toSlug(props.product.name)}`)

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

const incrementQuantity = async () => {
  try {
    await cartStore.increment(productId)
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

const removeItem = async () => {
  try {
    await cartStore.removeItem(productId)
  } catch (err) {
    console.error('Ошибка при удалении товара из корзины:', err)
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
