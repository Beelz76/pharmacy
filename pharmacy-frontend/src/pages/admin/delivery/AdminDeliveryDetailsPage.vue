<template>
  <div class="max-w-5xl mx-auto">
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

    <div v-if="loading" class="text-center py-10">Загрузка...</div>
    <div v-else-if="!delivery" class="text-center py-10 text-gray-500">
      Доставка не найдена
    </div>
    <div v-else class="space-y-6">
      <div class="bg-white border rounded-xl p-6 shadow-sm space-y-3">
        <p class="text-base text-gray-700">
          <span class="font-medium">ID:</span> {{ delivery.id }}
        </p>
        <p class="text-base text-gray-700">
          <span class="font-medium">Номер заказа:</span>
          {{ delivery.orderNumber }}
        </p>
        <p class="text-base text-gray-700">
          <span class="font-medium">Адрес:</span> {{ delivery.address }}
        </p>
        <p class="text-base text-gray-700">
          <span class="font-medium">Дата:</span>
          {{ formatDate(delivery.deliveryDate) }}
        </p>
        <p v-if="delivery.comment" class="text-base text-gray-700">
          <span class="font-medium">Комментарий:</span> {{ delivery.comment }}
        </p>
        <button
          class="text-primary-600 hover:text-primary-700 text-base underline"
          @click="goOrder(delivery.orderId)"
        >
          Перейти к заказу
        </button>
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
