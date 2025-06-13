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
        Доставка заказа №{{ delivery?.orderNumber }}
      </h2>
    </div>

    <div v-if="loading" class="text-center py-10 text-gray-500 text-lg">
      <i class="fas fa-spinner fa-spin mr-2"></i>Загрузка информации о
      доставке...
    </div>

    <div v-else-if="!delivery" class="text-center py-10 text-gray-500 text-lg">
      <i class="fas fa-exclamation-circle mr-2"></i>Доставка не найдена
    </div>

    <div v-else class="space-y-6">
      <div class="bg-white border rounded-xl p-6 shadow-sm">
        <div
          class="grid grid-cols-1 sm:grid-cols-2 gap-y-4 gap-x-8 text-gray-700"
        >
          <p><span class="font-medium">ID:</span> {{ delivery.id }}</p>
          <p>
            <span class="font-medium">Номер заказа:</span>
            {{ delivery.orderNumber }}
          </p>
          <p>
            <span class="font-medium">Дата:</span>
            {{ formatDate(delivery.deliveryDate) }}
          </p>
          <p class="sm:col-span-2">
            <span class="font-medium">Адрес:</span> {{ delivery.address }}
          </p>
          <p>
            <span class="font-medium">Стоимость:</span>
            {{ delivery.price.toFixed(2) }} ₽
          </p>
          <p v-if="delivery.comment" class="sm:col-span-2">
            <span class="font-medium">Комментарий:</span> {{ delivery.comment }}
          </p>
        </div>

        <div class="mt-6 text-right">
          <el-button type="primary" @click="goOrder(delivery.orderId)">
            <i class="fas fa-receipt mr-2"></i>Перейти к заказу
          </el-button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import { useRoute, useRouter } from "vue-router";
import { getDeliveryByOrderId } from "/src/services/DeliveryService";

const router = useRouter();
const route = useRoute();
const delivery = ref(null);
const loading = ref(false);

const formatDate = (iso) => (iso ? new Date(iso).toLocaleString("ru-RU") : "");

const goOrder = (id) => {
  router.push({ name: "AdminOrderDetails", params: { id } });
};

onMounted(async () => {
  loading.value = true;
  try {
    delivery.value = await getDeliveryByOrderId(route.params.orderId);
  } finally {
    loading.value = false;
  }
});
</script>
