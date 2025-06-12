<template>
  <div class="max-w-4xl mx-auto py-10 px-4">
    <!-- Назад и заголовок -->
    <div class="flex items-center gap-4 mb-8">
      <router-link
        :to="{ name: 'OrderCheckout' }"
        class="flex items-center text-primary-600 hover:text-primary-700 text-base group"
      >
        <i
          class="text-xl fas fa-arrow-left mr-2 group-hover:-translate-x-1 duration-150"
        ></i>
      </router-link>
      <h2 class="text-2xl font-bold tracking-tight">Подтверждение заказа</h2>
    </div>

    <div class="grid grid-cols-1 lg:grid-cols-2 gap-8">
      <!-- Информация о заказе -->
      <div
        class="bg-white border border-gray-200 rounded-xl p-6 shadow-sm space-y-5"
      >
        <h3 class="text-lg font-semibold text-gray-800">
          <i class="fas fa-file-alt mr-2 text-gray-400"></i>Информация о заказе
        </h3>

        <template v-if="isDelivery">
          <p class="text-sm text-gray-500 uppercase tracking-wide">
            Адрес доставки
          </p>
          <p class="text-base font-medium text-gray-900">
            {{ selectedAddress?.fullAddress }}
          </p>
          <p v-if="selectedAddress?.comment" class="text-sm text-gray-600 mt-1">
            {{ selectedAddress.comment }}
          </p>
          <div v-if="!hasAccountPhone" class="mt-2">
            <PhoneInput v-model="phone" digits-only />
          </div>
        </template>
        <template v-else>
          <p class="text-sm text-gray-500 uppercase tracking-wide">Аптека</p>
          <p class="text-base font-medium text-gray-900">
            «{{ selectedPharmacy.name }}»
          </p>
          <p class="text-sm text-gray-600">{{ selectedPharmacy.address }}</p>
        </template>
        <p v-if="deliveryComment" class="text-sm text-gray-600 mt-1">
          {{ deliveryComment }}
        </p>
        <div v-if="paymentMethod" class="pt-4">
          <p class="text-sm text-gray-500 uppercase tracking-wide">Оплата</p>
          <p class="text-base font-medium text-gray-900">
            {{ paymentMethod === "Online" ? "Картой онлайн" : "При получении" }}
          </p>
        </div>
      </div>

      <!-- Состав заказа -->
      <div
        class="bg-white border border-gray-200 rounded-xl p-6 shadow-sm space-y-5"
      >
        <h3 class="text-lg font-semibold text-gray-800">
          <i class="fas fa-box mr-2 text-gray-400"></i>Состав заказа
        </h3>

        <!-- Прокручиваемый список -->
        <div
          class="custom-scroll divide-y divide-gray-100 max-h-[360px] overflow-y-auto pr-3 -mr-3"
        >
          <div
            v-for="item in cartItems"
            :key="item.productId"
            class="py-4 flex justify-between items-start text-sm"
          >
            <div class="pr-4">
              <p class="font-medium text-gray-900">{{ item.name }}</p>
              <p class="text-gray-500 mt-1">{{ item.description }}</p>
            </div>
            <div class="text-right whitespace-nowrap">
              <p class="text-gray-500">×{{ item.quantity }}</p>
              <p class="font-semibold text-gray-900 text-base">
                {{ item.totalPrice.toFixed(2) }} ₽
              </p>
            </div>
          </div>
        </div>

        <div class="pt-4 border-t text-right text-xl font-bold text-gray-900">
          Итого: {{ totalPrice.toFixed(2) }} ₽
        </div>
      </div>
    </div>

    <!-- Кнопка подтверждения -->
    <div class="text-right mt-10">
      <el-button
        type="primary"
        size="large"
        :loading="loading"
        @click="submitOrder"
        class="!bg-primary-600 hover:!bg-primary-700 !px-10 !py-3 rounded-lg text-base"
      >
        Оформить заказ
      </el-button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, watch } from "vue";
import { useRouter } from "vue-router";
import { useCartStore } from "../../stores/CartStore";
import { useOrderStore } from "../../stores/OrderStore";
import api from "../../utils/axios";
import { useAccountStore } from "../../stores/AccountStore";
import PhoneInput from "../../components/inputs/PhoneInput.vue";
import { ElMessage } from "element-plus";

const router = useRouter();
const cartStore = useCartStore();
const orderStore = useOrderStore();
const accountStore = useAccountStore();

onMounted(() => {
  accountStore.fetchProfile().then(() => {
    if (accountStore.account?.phone) {
      phone.value = accountStore.account.phone;
    }
  });
});

watch(
  () => accountStore.account?.phone,
  (val) => {
    if (val) phone.value = val;
  }
);

const phone = ref("");

const loading = ref(false);

const selectedPharmacy = orderStore.selectedPharmacy;
const selectedAddressId = orderStore.selectedAddressId;
const selectedAddress = orderStore.selectedAddress;
const isDelivery = orderStore.isDelivery;
const paymentMethod = orderStore.paymentMethod;
const deliveryComment = orderStore.deliveryComment;
const cartItems = cartStore.items;
const totalPrice = cartStore.totalPrice;
const hasAccountPhone = computed(() => !!accountStore.account?.phone);

const fullPharmacyAddress = computed(() =>
  selectedPharmacy ? selectedPharmacy.address : ""
);

const submitOrder = async () => {
  try {
    loading.value = true;

    const payload = {
      paymentMethod,
      isDelivery,
      deliveryComment,
    };
    if (isDelivery) {
      payload.userAddressId = selectedAddressId;
      const contactPhone = hasAccountPhone.value
        ? accountStore.account.phone
        : phone.value;
      if (!contactPhone) {
        ElMessage({ message: "Укажите номер телефона", type: "warning" });
        loading.value = false;
        return;
      }
      if (!hasAccountPhone.value) {
        payload.phone = phone.value;
      }
    } else {
      const existRes = await api.post("/pharmacy/existing", {
        name: selectedPharmacy.name,
        osmId:
          selectedPharmacy.osmId?.toString() || selectedPharmacy.id?.toString(),
        latitude: selectedPharmacy.lat,
        longitude: selectedPharmacy.lon,
      });
      const pharmacyId = existRes.data;
      if (pharmacyId) {
        payload.pharmacyId = pharmacyId;
      } else {
        payload.newPharmacy = {
          name: selectedPharmacy.name,
          phone: selectedPharmacy.phone || null,
          address: {
            osmId:
              (selectedPharmacy.osmId || selectedPharmacy.id)?.toString() ||
              null,
            region:
              selectedPharmacy.addressData?.region ||
              selectedPharmacy.addressData?.state ||
              null,
            state: selectedPharmacy.addressData?.state || null,
            city:
              selectedPharmacy.addressData?.city ||
              selectedPharmacy.addressData?.town ||
              selectedPharmacy.addressData?.village ||
              null,
            suburb: selectedPharmacy.addressData?.suburb || null,
            street: selectedPharmacy.addressData?.road || null,
            houseNumber: selectedPharmacy.addressData?.house_number || null,
            postcode: selectedPharmacy.addressData?.postcode || null,
            latitude: selectedPharmacy.lat,
            longitude: selectedPharmacy.lon,
          },
        };
      }
    }

    const response = await api.post("/orders", payload);

    cartStore.resetCart();
    const { id, number } = response.data;

    orderStore.setCreatedOrder({
      id,
      number,
      total: totalPrice,
    });

    if (paymentMethod === "OnDelivery") {
      orderStore.resetOrder();
      router.push({ name: "OrderDetails", params: { id } });
    } else {
      try {
        const payRes = await api.post(`/orders/${id}/pay`);
        orderStore.resetOrder();
        const url = payRes.data;
        if (url && /^https?:\/\//.test(url)) {
          window.location.href = url;
        } else {
          console.error("Неверная ссылка для оплаты", url);
        }
      } catch (e) {
        console.error("Ошибка оплаты:", e);
      }
    }
  } catch (error) {
    console.error("Ошибка при оформлении заказа:", error);
  } finally {
    loading.value = false;
  }
};
</script>

<style scoped>
.el-button {
  transition: background-color 0.2s ease-in-out;
}
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
