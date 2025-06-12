<template>
  <div class="max-w-3xl mx-auto">
    <div class="flex items-center gap-4 mb-8">
      <button
        @click="router.back()"
        class="flex items-center text-primary-600 hover:text-primary-700 text-base group transition"
      >
        <i
          class="text-xl fas fa-arrow-left mr-2 group-hover:-translate-x-1 duration-150"
        ></i>
      </button>
      <h2 class="text-2xl font-bold tracking-tight">
        Товар {{ product?.name }}
      </h2>
    </div>

    <div v-if="loading" class="text-center py-10 text-gray-500 text-lg">
      <i class="fas fa-spinner fa-spin mr-2"></i>Загрузка информации...
    </div>

    <div v-else-if="!product" class="text-center py-10 text-gray-500 text-lg">
      <i class="fas fa-exclamation-circle mr-2"></i>Товар не найден
    </div>

    <div v-else class="bg-white border rounded-xl p-6 shadow-sm space-y-6">
      <div
        class="grid grid-cols-1 sm:grid-cols-2 gap-y-4 gap-x-8 text-gray-700"
      >
        <p><span class="font-medium">Название:</span> {{ product.name }}</p>
        <p>
          <span class="font-medium">Категория:</span>
          {{ fullCategoryName }}
        </p>
        <p>
          <span class="font-medium">Производитель:</span>
          {{ product.manufacturer.name }} ({{ product.manufacturer.country }})
        </p>
        <p>
          <span class="font-medium">Цена:</span>
          {{ product.price.toFixed(2) }} ₽
        </p>
        <p>
          <span class="font-medium">Доступен:</span>
          {{ product.isAvailable ? "Да" : "Нет" }}
        </p>
      </div>

      <div v-if="product.description" class="pt-4">
        <h3 class="text-lg font-semibold text-gray-800 mb-2">
          Краткое описание
        </h3>
        <p class="text-sm text-gray-700 whitespace-pre-line">
          {{ product.description }}
        </p>
      </div>

      <div v-if="product.extendedDescription" class="pt-4">
        <h3 class="text-lg font-semibold text-gray-800 mb-2">Описание</h3>
        <p class="text-sm text-gray-700 whitespace-pre-line">
          {{ product.extendedDescription }}
        </p>
      </div>

      <div v-if="normalizedProperties.length" class="pt-4">
        <h3 class="text-lg font-semibold text-gray-800 mb-2">Характеристики</h3>
        <ul class="space-y-1 text-sm text-gray-700">
          <li v-for="prop in normalizedProperties" :key="prop.key">
            <span class="font-medium"
              >{{ prop.label || formatKey(prop.key) }}:</span
            >
            {{ prop.value }}
          </li>
        </ul>
      </div>

      <div v-if="product.images?.length" class="pt-4">
        <h3 class="text-lg font-semibold text-gray-800 mb-2">Изображения</h3>
        <div class="flex flex-wrap gap-3">
          <img
            v-for="img in product.images"
            :key="img.id"
            :src="img.url"
            class="w-24 h-24 object-cover border rounded"
          />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from "vue";
import { useRoute, useRouter } from "vue-router";
import ProductService from "/src/services/ProductService";

const router = useRouter();
const route = useRoute();
const product = ref(null);
const loading = ref(false);

const fullCategoryName = computed(() => {
  if (!product.value) return "";
  if (product.value.parentCategory?.name) {
    return `${product.value.parentCategory.name} / ${product.value.category.name}`;
  }
  return product.value.category.name;
});

const normalizedProperties = computed(() =>
  (product.value?.properties || []).map((p) => ({
    key: p.key ?? p.Key,
    label: p.label ?? p.Label,
    value: p.value ?? p.Value,
  }))
);

const formatKey = (key) => {
  const map = {
    form: "Форма",
    dosage: "Дозировка",
    composition: "Состав",
    instruction: "Инструкция",
  };
  return map[key] || key;
};

onMounted(async () => {
  loading.value = true;
  try {
    const id = Number(route.params.id);
    if (id) {
      product.value = await ProductService.getById(id);
    }
  } catch {
    product.value = null;
  } finally {
    loading.value = false;
  }
});
</script>
