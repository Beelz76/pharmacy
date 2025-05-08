<template>
  <div class="bg-white rounded-xl shadow hover:shadow-lg transition overflow-hidden relative flex flex-col">
    <!-- Изображение -->
    <div class="relative h-48 bg-gray-100 flex items-center justify-center overflow-hidden">
      <img
        :src="product.imageUrl || defaultImage"
        alt="product image"
        class="w-full h-full object-cover"
      />

      <!-- Иконка избранного -->
      <button
        @click="$emit('toggle-favorite', product.id)"
        class="absolute top-2 right-2 text-xl"
      >
        <i
          :class="[
            'fas fa-heart',
            product.isFavorite ? 'text-red-500' : 'text-gray-300 hover:text-red-500'
          ]"
        ></i>
      </button>

      <!-- Плашка "По рецепту" -->
      <div
        v-if="product.isPrescriptionRequired"
        class="absolute top-2 left-2 bg-blue-100 text-primary-600 text-xs font-medium px-2 py-0.5 rounded inline-flex items-center gap-1"
      >
        <i class="fas fa-file-prescription"></i>
        По рецепту
      </div>

      <!-- Плашка "Нет в наличии" -->
      <div
        v-if="!product.isAvailable"
        class="absolute bottom-2 left-2 bg-gray-300 text-gray-800 text-xs font-medium px-2 py-0.5 rounded"
      >
        Нет в наличии
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

        <div v-if="product.cartQuantity > 0" class="flex items-center gap-2">
          <button
            @click="$emit('decrement', product.id)"
            class="px-2 py-1 bg-gray-200 rounded hover:bg-gray-300"
          >−</button>
          <span class="text-sm font-medium">{{ product.cartQuantity }}</span>
          <button
            @click="$emit('increment', product.id)"
            class="px-2 py-1 bg-gray-200 rounded hover:bg-gray-300"
          >+</button>
        </div>

        <button
          v-else
          :disabled="!product.isAvailable"
          @click="$emit('add-to-cart', product.id)"
          class="bg-primary-600 text-white px-3 py-1 rounded text-sm hover:bg-primary-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
        >
          В корзину
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
const defaultImage = 'https://via.placeholder.com/300x200?text=No+Image'

defineProps({
  product: {
    type: Object,
    required: true
  }
})

defineEmits(['add-to-cart', 'increment', 'decrement', 'toggle-favorite'])
</script>
