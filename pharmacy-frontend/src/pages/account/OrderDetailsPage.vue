<template>
  <div class="max-w-6xl mx-auto">
    <!-- Назад и заголовок -->
    <div class="flex items-center gap-4 mb-8">
      <button
        @click="goBack"
        class="flex items-center text-primary-600 hover:text-primary-700 text-base group transition"
      >
        <i
          class="text-xl fas fa-arrow-left mr-2 group-hover:-translate-x-1 duration-150"
        ></i>
      </button>
      <h2 class="text-2xl font-bold tracking-tight">
        Заказ №{{ order?.number }}
      </h2>
    </div>

    <div v-if="!auth.isAuthenticated" class="text-center text-gray-500 mb-4">
      Необходимо авторизоваться
    </div>

    <div
      v-if="auth.isAuthenticated"
      class="grid grid-cols-1 lg:grid-cols-3 gap-8"
    >
      <!-- Левая часть -->
      <div class="lg:col-span-2 space-y-6">
        <!-- Дата и статус -->
        <div class="flex items-center justify-between text-sm text-gray-600">
          <span>Дата оформления: {{ formatDate(order?.createdAt) }}</span>
          <span
            class="inline-block px-3 py-2 text-xs rounded-full font-semibold tracking-wide"
            :class="statusClass(order?.status)"
          >
            {{ order?.status }}
          </span>
        </div>

        <!-- Адрес -->
        <div class="bg-white border rounded-xl p-6 shadow-sm space-y-3">
          <h3
            class="text-sm font-semibold text-gray-700 uppercase tracking-wide"
          >
            <i class="fas fa-map-marker-alt mr-1 text-gray-400"></i>
            {{ order?.isDelivery ? "Адрес доставки" : "Аптека" }}
          </h3>
          <template v-if="order?.isDelivery">
            <p class="text-sm text-gray-600">{{ order.deliveryAddress }}</p>
          </template>
          <template v-else>
            <p class="text-base font-medium text-gray-900">
              «{{ pharmacyName }}»
            </p>
            <p class="text-sm text-gray-600">{{ pharmacyAddress }}</p>

            <div v-if="order?.pickupCode" class="pt-4">
              <h4 class="text-sm font-semibold text-gray-700 mb-1">
                Код получения
              </h4>
              <div
                class="inline-block border border-dashed border-gray-400 rounded-lg px-4 py-2 text-xl font-bold tracking-wider text-gray-800 bg-gray-50"
              >
                {{ order.pickupCode }}
              </div>
            </div>
          </template>
        </div>

        <!-- Состав заказа -->
        <div class="bg-white border rounded-xl shadow-sm overflow-hidden">
          <h3
            class="px-6 py-4 border-b text-lg font-semibold text-gray-800 bg-gray-50"
          >
            Состав заказа
          </h3>

          <ul
            class="divide-y divide-gray-100 max-h-[360px] overflow-y-auto pr-1 custom-scroll"
          >
            <li
              v-for="item in order?.items"
              :key="item.productId"
              class="px-6 py-4 flex justify-between items-start text-sm"
            >
              <div>
                <p class="font-medium text-gray-900 flex items-center gap-2">
                  {{ item.productName }}
                  <button
                    @click="goProduct(item.productId, item.productName)"
                    class="text-primary-600 hover:text-primary-700"
                  >
                    <i class="fas fa-link"></i>
                  </button>
                </p>
                <p class="text-gray-500">Количество: {{ item.quantity }}</p>
              </div>
              <p
                class="text-right font-semibold text-gray-800 whitespace-nowrap"
              >
                {{ item.price.toFixed(2) }} ₽
              </p>
            </li>
          </ul>

          <div
            class="px-6 py-4 border-t bg-gray-50 text-right font-bold text-xl text-gray-900"
          >
            Итого: {{ order?.totalPrice.toFixed(2) }} ₽
          </div>
          <div
            v-if="order?.status === 'Отменен' && order.cancellationComment"
            class="px-6 py-4 text-sm text-red-600 border-t bg-red-50"
          >
            <span class="font-semibold">Причина отмены:</span>
            {{ order.cancellationComment }}
          </div>
        </div>
      </div>

      <!-- Оплата -->
      <div class="bg-white border rounded-xl shadow-sm p-6 space-y-5">
        <h3 class="text-lg font-semibold text-gray-800">
          <i class="fas fa-credit-card mr-2 text-gray-400"></i>Оплата
        </h3>
        <div class="text-sm text-gray-700 space-y-2">
          <p>
            <span class="font-medium">Способ:</span> {{ order?.payment.method }}
          </p>
          <p>
            <span class="font-medium">Сумма:</span>
            {{ order?.payment.amount.toFixed(2) }} ₽
          </p>
          <p>
            <span class="font-medium">Статус:</span> {{ order?.payment.status }}
          </p>
        </div>

        <div v-if="order?.status === 'Ожидает оплаты'" class="pt-2">
          <el-button
            type="primary"
            size="large"
            class="w-full !bg-blue-500 hover:!bg-blue-600"
            @click="pay"
          >
            Перейти к оплате
          </el-button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from "vue";
import { useAuthStore } from "/src/stores/AuthStore";
import { useRoute, useRouter } from "vue-router";
import { getOrderById, payOrder } from "/src/services/OrderService";
import { useOrderStore } from "../../stores/OrderStore";
import { statusClass } from "../../utils/statusClass";
import { toSlug } from "../../utils/slugify";

const router = useRouter();
const orderStore = useOrderStore();
const route = useRoute();
const order = ref(null);
const loading = ref(false);
const auth = useAuthStore();

const orderId = route.params.id;

const goBack = () => {
  router.back();
};

const isDelivery = computed(() => order.value?.isDelivery);

const pharmacyName = computed(() => {
  if (!order.value?.pharmacyAddress) return "";
  const parts = order.value.pharmacyAddress.split(",");
  return parts[parts.length - 1].trim();
});

const pharmacyAddress = computed(() => {
  if (!order.value?.pharmacyAddress) return "";
  const parts = order.value.pharmacyAddress.split(",");
  return parts.slice(0, -1).join(",").trim();
});

const formatDate = (isoString) => {
  return new Date(isoString).toLocaleString("ru-RU", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });
};

onMounted(async () => {
  if (!auth.isAuthenticated) return;
  try {
    loading.value = true;
    order.value = await getOrderById(orderId);
  } finally {
    loading.value = false;
  }
});

const goProduct = (id, name) => {
  router.push({ name: "ProductDetails", params: { id, slug: toSlug(name) } });
};

const pay = async () => {
  try {
    const url = await payOrder(order.value.id);
    if (url && /^https?:\/\//.test(url)) {
      window.location.href = url;
    } else {
      console.error("Invalid payment url", url);
    }
  } catch (e) {
    console.error("Ошибка оплаты", e);
  }
};
</script>

<style scoped>
.custom-scroll::-webkit-scrollbar {
  width: 6px;
}
.custom-scroll::-webkit-scrollbar-thumb {
  background-color: rgba(0, 0, 0, 0.15);
  border-radius: 8px;
}
.custom-scroll::-webkit-scrollbar-track {
  background-color: transparent;
}
</style>
