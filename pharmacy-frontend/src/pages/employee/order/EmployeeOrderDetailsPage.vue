<template>
  <div class="max-w-5xl mx-auto">
    <div class="flex items-center gap-4 mb-8">
      <button
        @click="router.back()"
        class="flex items-center text-primary-600 hover:text-primary-700 text-base group transition"
      >
        <i
          class="text-xl fas fa-arrow-left mr-2 group-hover:-translate-x-1 duration-150"
        />
      </button>
      <h2 class="text-2xl font-bold tracking-tight flex items-center gap-2">
        Заказ №{{ order?.number }}
      </h2>
    </div>

    <div v-if="loading" class="text-center py-10">Загрузка...</div>
    <div v-else-if="!order" class="text-center py-10 text-gray-500">
      Заказ не найден
    </div>

    <template v-else>
      <div
        class="flex flex-wrap justify-between items-center text-sm text-gray-600 mb-4 gap-2"
      >
        <span> Дата оформления: {{ formatDate(order.createdAt) }} </span>
        <el-select
          v-model="order.status"
          size="medium"
          @change="changeStatus"
          class="status-select"
        >
          <el-option
            v-for="s in statuses"
            :key="s.id"
            :label="s.description"
            :value="s.name"
          />
        </el-select>
      </div>

      <div
        class="grid grid-cols-1 lg:grid-cols-3 gap-8 items-start max-h-[400px]"
      >
        <div class="lg:col-span-2 flex flex-col gap-6">
          <div
            class="bg-white border rounded-xl p-6 shadow-sm space-y-3 flex-1"
          >
            <h3
              class="text-sm font-semibold text-gray-700 uppercase tracking-wide flex items-center gap-2"
            >
              <i class="fas fa-map-marker-alt text-gray-400"></i>
              {{ order.isDelivery ? "Адрес доставки" : "Аптека" }}
            </h3>
            <template v-if="order.isDelivery">
              <p class="text-sm text-gray-600">{{ order.deliveryAddress }}</p>
            </template>
            <template v-else>
              <p class="text-base font-medium text-gray-900">
                «{{ order.pharmacyName }}»
              </p>
              <p class="text-sm text-gray-600">{{ order.pharmacyAddress }}</p>
              <div v-if="order.pickupCode" class="pt-4">
                <h4 class="text-sm font-semibold text-gray-700 mb-1">
                  <i class="fas fa-key mr-1 text-gray-400"></i>Код получения
                </h4>
                <div
                  class="inline-block border border-dashed border-gray-400 rounded-lg px-4 py-2 text-xl font-bold tracking-wider text-gray-800 bg-gray-50"
                >
                  {{ order.pickupCode }}
                </div>
              </div>
            </template>
          </div>

          <div class="bg-white border rounded-xl shadow-sm overflow-hidden">
            <h3
              class="px-6 py-4 border-b text-lg font-semibold text-gray-800 bg-gray-50 flex items-center gap-2"
            >
              <i class="fas fa-box-open text-gray-400"></i>
              Состав заказа
            </h3>
            <ul
              class="divide-y divide-gray-100 max-h-[360px] overflow-y-auto pr-1 custom-scroll"
            >
              <li
                v-for="item in order.items"
                :key="item.productId"
                class="px-6 py-4 flex justify-between items-start text-sm"
              >
                <div>
                  <p class="font-medium text-gray-900 flex items-center gap-2">
                    {{ item.productName }}
                    <button
                      @click="goProduct(item.productId)"
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
              Итого: {{ order.totalPrice.toFixed(2) }} ₽
            </div>
            <div
              v-if="order?.status === 'Cancelled' && order.cancellationComment"
              class="px-6 py-4 text-sm text-red-600 border-t bg-red-50"
            >
              <span class="font-semibold">Причина отмены:</span>
              {{ order.cancellationComment }}
            </div>
          </div>
        </div>

        <div class="space-y-6 h-full flex flex-col">
          <div class="bg-white border rounded-xl p-6 space-y-3 flex-1">
            <h3
              class="text-lg font-semibold text-gray-800 flex items-center gap-2"
            >
              <i class="fas fa-user text-gray-400"></i>Покупатель
            </h3>
            <p class="text-sm text-gray-700">
              <span class="font-medium">ФИО:</span> {{ order.userFullName }}
            </p>
            <p class="text-sm text-gray-700">
              <span class="font-medium">Email:</span> {{ order.userEmail }}
            </p>
          </div>

          <div class="bg-white border rounded-xl shadow-sm p-6 space-y-5">
            <h3
              class="text-lg font-semibold text-gray-800 flex items-center gap-2"
            >
              <i class="fas fa-credit-card text-gray-400"></i>Оплата
            </h3>
            <div class="text-sm text-gray-700 space-y-2">
              <p>
                <span class="font-medium">Способ:</span>
                {{ order.payment.method }}
              </p>
              <p>
                <span class="font-medium">Сумма:</span>
                {{ order.payment.amount.toFixed(2) }} ₽
              </p>
              <p class="flex items-center gap-2">
                <span class="font-medium">Статус:</span>
                <el-select
                  v-model="order.paymentStatus"
                  size="small"
                  :disabled="order.isDelivery"
                  @change="changePaymentStatus"
                >
                  <el-option
                    v-for="s in paymentStatuses"
                    :key="s.id"
                    :label="s.description"
                    :value="s.name"
                  />
                </el-select>
              </p>
            </div>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from "vue";
import { useRoute, useRouter } from "vue-router";
import {
  getOrderById,
  getOrderStatuses,
  updateOrderStatus,
  cancelOrder,
} from "/src/services/OrderService";
import { getPaymentStatuses } from "/src/services/PaymentReferenceService";
import { updatePaymentStatus } from "/src/services/PaymentService";
import { ElMessage, ElMessageBox } from "element-plus";

const router = useRouter();
const route = useRoute();
const order = ref(null);
const loading = ref(false);
const statuses = ref([]);
const paymentStatuses = ref([]);

const orderId = route.params.id;

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
  try {
    loading.value = true;
    order.value = await getOrderById(orderId);
    statuses.value = await getOrderStatuses();
    paymentStatuses.value = await getPaymentStatuses();
    const current = statuses.value.find(
      (s) => s.description === order.value.status
    );
    if (current) order.value.status = current.name;
    const ps = paymentStatuses.value.find(
      (s) => s.description === order.value.payment.status
    );
    if (ps) order.value.paymentStatus = ps.name;
  } finally {
    loading.value = false;
  }
});

const changePaymentStatus = async () => {
  const prev = order.value.paymentStatus;
  try {
    await updatePaymentStatus(order.value.id, order.value.paymentStatus);
    ElMessage.success("Статус оплаты обновлён");
  } catch {
    order.value.paymentStatus = prev;
  }
};

const changeStatus = async () => {
  const prev = order.value.status;
  try {
    if (order.value.status === "Cancelled") {
      let comment = "";
      try {
        const { value } = await ElMessageBox.prompt(
          "Укажите причину отмены (опционально)",
          "Отмена заказа",
          {
            confirmButtonText: "Отменить",
            cancelButtonText: "Закрыть",
            draggable: true,
          }
        );
        comment = value;
      } catch {
        return;
      }
      await cancelOrder(order.value.id, comment || null);
      ElMessage.success("Заказ отменён");
      order.value.status = "Cancelled";
    } else {
      await updateOrderStatus(order.value.id, order.value.status);
      ElMessage.success("Статус обновлён");
    }
  } catch {
    order.value.status = prev;
  }
};

const goProduct = (id) => {
  router.push({ name: "EmployeeProductDetails", params: { id } });
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
.status-select {
  width: 100%;
}
@media (min-width: 1024px) {
  .status-select {
    width: calc(33.333% - 2rem);
  }
}
</style>
