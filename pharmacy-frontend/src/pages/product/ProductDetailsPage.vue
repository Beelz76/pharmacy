<template>
  <div class="max-w-7xl mx-auto px-4 py-8">
    <!-- Хлебные крошки -->
    <nav v-if="product" class="text-sm text-gray-500 mb-6">
      <router-link to="/" class="hover:underline">Главная</router-link> /
      <router-link to="/products/catalog" class="hover:underline"
        >Каталог</router-link
      >
      /
      <router-link :to="{ name: 'ProductsByCategory' }" class="hover:underline">
        {{ product.category.name }}
      </router-link>
      / <span class="text-gray-800">{{ product.name }}</span>
    </nav>

    <div v-if="loading" class="flex justify-center mt-12">
      <LoadingSpinner size="lg" color="primary" class="mx-auto" />
    </div>

    <div
      v-else-if="product"
      class="grid grid-cols-1 lg:grid-cols-[350px_1fr] gap-8 bg-white p-6 rounded-xl shadow"
    >
      <!-- Левая колонка -->
      <div class="space-y-4">
        <!-- Галерея изображений -->
        <div class="space-y-2">
          <div
            class="border rounded-xl h-[300px] flex justify-center items-center bg-white overflow-hidden cursor-pointer"
            @click="showModal = true"
          >
            <template v-if="activeImage">
              <img
                :src="activeImage"
                class="max-h-full object-contain"
                :alt="product.name"
              />
            </template>
            <template v-else>
              <i class="fas fa-image text-5xl text-gray-300"></i>
            </template>
          </div>
          <div v-if="product.images?.length > 1" class="flex gap-2">
            <img
              v-for="(img, index) in product.images"
              :key="index"
              :src="img"
              class="w-16 h-16 object-contain border rounded cursor-pointer hover:ring-2"
              @click="activeImage = img"
            />
          </div>
        </div>

        <!-- Метки -->
        <div class="flex flex-wrap gap-2 text-sm">
          <span
            v-if="product.isPrescriptionRequired"
            class="inline-flex items-center px-3 py-1 rounded-full bg-blue-100 text-blue-800 font-medium"
          >
            <i class="fas fa-prescription-bottle-alt mr-1"></i> По рецепту
          </span>
          <span
            :class="
              product.isAvailable
                ? 'bg-green-100 text-green-800'
                : 'bg-gray-200 text-gray-600'
            "
            class="inline-flex items-center px-3 py-1 rounded-full font-medium"
          >
            {{ product.isAvailable ? "В наличии" : "Нет в наличии" }}
          </span>
        </div>

        <!-- Цена и кнопки -->
        <div
          class="bg-white border rounded-xl shadow-sm p-4 space-y-4 sticky top-24"
        >
          <div class="text-2xl font-bold text-gray-900">
            {{ product.price.toFixed(2) }} ₽
          </div>

          <div class="flex items-center gap-3">
            <div v-if="cartQuantity > 0" class="flex items-center gap-1">
              <button
                @click="decrementQuantity"
                class="w-8 h-8 bg-gray-200 rounded hover:bg-gray-300"
              >
                <i class="fas fa-minus text-xs"></i>
              </button>
              <input
                v-model.number="editableQuantity"
                type="number"
                min="1"
                class="w-12 h-8 text-center border rounded text-sm"
                @change="setQuantity"
              />
              <button
                @click="incrementQuantity"
                class="w-8 h-8 bg-gray-200 rounded hover:bg-gray-300"
              >
                <i class="fas fa-plus text-xs"></i>
              </button>
            </div>

            <el-button
              v-else
              type="primary"
              class="!h-10 !px-6"
              :disabled="!product.isAvailable"
              @click="addToCart"
            >
              <i class="fas fa-cart-plus mr-2"></i> В корзину
            </el-button>

            <el-button plain class="!h-10" @click="toggleFavorite">
              <i
                :class="
                  isFavorite
                    ? 'fas fa-heart text-red-500'
                    : 'fas fa-heart text-gray-400 hover:text-red-500'
                "
              ></i>
            </el-button>
          </div>
        </div>
      </div>

      <!-- Правая колонка -->
      <div class="space-y-6">
        <h1 class="text-3xl font-bold text-gray-900">{{ product.name }}</h1>
        <p class="text-gray-700 text-base">{{ product.description }}</p>

        <!-- Метаданные -->
        <div
          class="bg-gray-50 p-4 rounded-lg border grid sm:grid-cols-2 gap-4 text-sm text-gray-600"
        >
          <div>
            <strong>Категория:</strong>
            <router-link :to="categoryLink" class="text-primary-600">
              {{ product.category.name }}
            </router-link>
          </div>
          <div><strong>Артикул:</strong> {{ product.sku }}</div>
          <div>
            <strong>Производитель:</strong>
            <router-link
              :to="manufacturerLink"
              class="text-primary-600 hover:underline"
            >
              {{ product.manufacturer.name }}
            </router-link>
          </div>
          <div>
            <strong>Страна:</strong>
            <router-link
              :to="countryLink"
              class="text-primary-600 hover:underline"
            >
              {{ product.manufacturer.country }}
            </router-link>
          </div>
          <div v-if="product.expirationDate">
            <strong>Срок годности:</strong>
            {{ formatRemainingShelfLife(product.expirationDate) }}
          </div>
        </div>

        <!-- Состав -->
        <div
          v-if="getProperty('composition')"
          class="bg-gray-50 p-4 rounded-lg border"
        >
          <h3 class="text-lg font-semibold text-gray-800 mb-2">Состав</h3>
          <p class="text-sm text-gray-700 whitespace-pre-line">
            {{ getProperty("composition") }}
          </p>
        </div>

        <!-- Инструкция -->
        <div
          v-if="getProperty('instruction')"
          class="bg-gray-50 p-4 rounded-lg border"
        >
          <h3 class="text-lg font-semibold text-gray-800 mb-2">Инструкция</h3>
          <p class="text-sm text-gray-700 whitespace-pre-line">
            {{ getProperty("instruction") }}
          </p>
        </div>

        <!-- Характеристики -->
        <div
          v-if="filteredProperties.length"
          class="bg-gray-50 p-4 rounded-lg border"
        >
          <h3 class="text-lg font-semibold text-gray-800 mb-2">
            Характеристики
          </h3>
          <ul class="space-y-1 text-sm text-gray-700">
            <li v-for="prop in filteredProperties" :key="prop.key">
              <span class="font-medium"
                >{{ prop.label || formatKey(prop.key) }}:</span
              >
              {{ prop.value }}
            </li>
          </ul>
        </div>

        <!-- Описание -->
        <div
          v-if="product.extendedDescription"
          class="bg-gray-50 p-4 rounded-lg border"
        >
          <h3 class="text-lg font-semibold text-gray-800 mb-2">Описание</h3>
          <p class="text-sm text-gray-700 whitespace-pre-line">
            {{ product.extendedDescription }}
          </p>
        </div>
      </div>
    </div>

    <div v-else class="text-center py-16 text-gray-500">Товар не найден</div>

    <!-- Модалка увеличенного изображения (по желанию можно позже улучшить) -->
    <el-dialog v-model="showModal" width="600px" align-center>
      <img :src="activeImage" class="w-full h-auto object-contain" />
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, onMounted, computed, watch } from "vue";
import { useRoute } from "vue-router";
import ProductService from "../../services/ProductService";
import { useCartStore } from "/src/stores/CartStore";
import { useFavoritesStore } from "/src/stores/FavoritesStore";
import LoadingSpinner from "../../components/LoadingSpinner.vue";
import { toSlug } from "../../utils/slugify";

const route = useRoute();
const product = ref(null);
const loading = ref(true);
const activeImage = ref("");

const cartStore = useCartStore();
const favoritesStore = useFavoritesStore();

const cartQuantity = computed(() =>
  product.value ? cartStore.quantityById[product.value.id] || 0 : 0
);

const editableQuantity = computed({
  get: () => cartQuantity.value || 1,
  set: async (val) => {
    const quantity = Number(val);
    if (!Number.isInteger(quantity) || quantity < 1) return;
    try {
      await cartStore.setQuantity(product.value.id, quantity);
    } catch (err) {
      console.error("Ошибка при изменении количества:", err);
    }
  },
});

const isFavorite = computed(() =>
  product.value ? favoritesStore.ids.includes(product.value.id) : false
);

const categoryLink = computed(() =>
  product.value
    ? {
        name: "ProductsByCategory",
        params: { slug: toSlug(product.value.category.name) },
      }
    : { name: "Products" }
);

const manufacturerLink = computed(() => ({
  path: "/products/catalog",
  query: { manufacturerIds: product.value?.manufacturer.id },
}));

const countryLink = computed(() => ({
  path: "/products/catalog",
  query: { countries: product.value?.manufacturer.country },
}));

onMounted(async () => {
  try {
    const id = Number(route.params.id);
    if (!id) throw new Error("Неверный ID");
    product.value = await ProductService.getById(id);
    activeImage.value = product.value?.images[0] || "";
  } catch (error) {
    console.error("Ошибка загрузки товара:", error);
  } finally {
    loading.value = false;
  }
});

const getProperty = (key) => {
  if (!product.value?.properties) return undefined;
  const prop = product.value.properties.find((p) => (p.key ?? p.Key) === key);
  return prop ? prop.value ?? prop.Value : undefined;
};

const formatKey = (key) => {
  const map = {
    form: "Форма",
    dosage: "Дозировка",
    composition: "Состав",
    instruction: "Инструкция",
  };
  return map[key] || key;
};

const normalizedProperties = computed(() =>
  (product.value?.properties || []).map((p) => ({
    key: p.key ?? p.Key,
    label: p.label ?? p.Label,
    value: p.value ?? p.Value,
  }))
);

const filteredProperties = computed(() =>
  normalizedProperties.value.filter(
    (prop) => prop.key && !["composition", "instruction"].includes(prop.key)
  )
);

const formatRemainingShelfLife = (expirationDate) => {
  const now = new Date();
  const exp = new Date(expirationDate);

  let months =
    (exp.getFullYear() - now.getFullYear()) * 12 +
    (exp.getMonth() - now.getMonth());
  const isPast = exp < now;

  if (isPast) return "Истёк";

  const years = Math.floor(months / 12);
  months = months % 12;

  const parts = [];
  if (years > 0)
    parts.push(`${years} ${years === 1 ? "год" : years < 5 ? "года" : "лет"}`);
  if (months > 0)
    parts.push(
      `${months} ${months === 1 ? "месяц" : months < 5 ? "месяца" : "месяцев"}`
    );

  return parts.join(" ");
};

const formatDate = (dateString) => {
  return new Date(dateString).toLocaleDateString("ru-RU");
};

const addToCart = async () => {
  try {
    await cartStore.addToCart(product.value.id, product.value);
  } catch (err) {
    console.error("Ошибка при добавлении в корзину:", err);
  }
};

const incrementQuantity = async () => {
  try {
    await cartStore.increment(product.value.id, product.value);
  } catch (err) {
    console.error("Ошибка при увеличении количества:", err);
  }
};

const decrementQuantity = async () => {
  try {
    await cartStore.decrement(product.value.id);
  } catch (err) {
    console.error("Ошибка при уменьшении количества:", err);
  }
};

const toggleFavorite = async () => {
  try {
    await favoritesStore.toggle(product.value.id, isFavorite.value);
  } catch (err) {
    console.error("Ошибка при обновлении избранного:", err);
  }
};
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
