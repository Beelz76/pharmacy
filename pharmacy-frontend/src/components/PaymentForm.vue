<template>
  <div :class="containerClass">
    <!-- Заголовок + сумма (вне формы) -->
    <div class="bg-white shadow-md rounded-xl p-6 mb-6 text-center">
      <h2 class="text-2xl font-bold">Оплата по заказу</h2>
      <span class="text-2xl font-bold text-primary-600">{{ orderNumber }}</span>
      <p class="text-gray-600 mt-2">
        Сумма к оплате: <strong>{{ totalPrice }} ₽</strong>
      </p>
    </div>

    <!-- Tabs for payment method -->
    <div class="bg-white shadow-md rounded-xl p-6">
      <div class="flex mb-6 border-b border-gray-200">
        <button
          class="flex-1 py-2 text-center text-sm font-medium transition-colors duration-150"
          :class="activeTab === 'card' ? 'border-b-2 border-blue-500 text-blue-600' : 'text-gray-500 hover:text-blue-600'"
          @click="activeTab = 'card'"
        >
          Оплата картой
        </button>
        <button
          class="flex-1 py-2 text-center text-sm font-medium transition-colors duration-150"
          :class="activeTab === 'qr' ? 'border-b-2 border-blue-500 text-blue-600' : 'text-gray-500 hover:text-blue-600'"
          @click="activeTab = 'qr'"
        >
          QR-код
        </button>
      </div>

      <!-- QR-код и отметка -->
      <div v-if="activeTab === 'qr'" class="space-y-4">
        <div class="flex justify-center">
          <canvas ref="qrCanvas" width="256" height="256" class="border rounded" />
        </div>
        <el-checkbox v-model="hasPaidViaQr" class="block text-center">
          Я оплатил через QR-код
        </el-checkbox>
      </div>

      <!-- Форма карты -->
      <el-form
        v-if="activeTab === 'card'"
        :model="form"
        :rules="rules"
        ref="formRef"
        label-position="top"
        class="space-y-4"
      >
        <el-form-item label="Номер карты" prop="number">
          <el-input
            v-model="form.number"
            placeholder="1234 5678 9012 3456"
            maxlength="19"
            @input="formatCardNumber"
          />
        </el-form-item>

        <el-form-item label="Владелец" prop="name">
          <el-input v-model="form.name" placeholder="Имя и Фамилия" />
        </el-form-item>

        <div class="grid grid-cols-2 gap-4">
          <el-form-item label="Действует до" prop="expiry">
            <el-input
              v-model="form.expiry"
              placeholder="MM/YY"
              maxlength="5"
              @input="formatExpiry"
            />
          </el-form-item>

          <el-form-item label="CVC/CVV" prop="cvv">
            <el-input v-model="form.cvv" placeholder="123" maxlength="3" />
          </el-form-item>
        </div>
      </el-form>

      <!-- Кнопка -->
      <div class="text-center mt-6">
        <el-button
          type="primary"
          size="large"
          :loading="loading"
          class="w-full !bg-blue-500 hover:!bg-blue-600"
          :disabled="activeTab === 'qr' && !hasPaidViaQr"
          @click="submit"
        >
          Оплатить {{ totalPrice }} ₽
        </el-button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, watch, nextTick, computed } from 'vue'
import { useRouter } from 'vue-router'
import api from '../utils/axios'
import { ElMessage } from 'element-plus'
import { useOrderStore } from '../stores/OrderStore'
import QRCode from 'qrcode'

const router = useRouter()
const orderStore = useOrderStore()
const orderId = orderStore.orderId
const orderNumber = ref(orderStore.orderNumber)
const totalPrice = ref(orderStore.orderTotal?.toFixed(2) || '...')
const activeTab = ref('card')
const hasPaidViaQr = ref(false)
const qrCanvas = ref(null)
const loading = ref(false)
const isFromSummary = ref(false)

const formRef = ref()
const form = ref({
  number: '',
  name: '',
  expiry: '',
  cvv: ''
})

const containerClass = computed(() =>
  isFromSummary.value ? 'max-w-md mx-auto py-10 px-4' : 'max-w-md mx-auto'
)

const rules = {
  number: [
    { required: true, message: 'Введите номер карты', trigger: 'blur' },
    {
      pattern: /^\d{4} \d{4} \d{4} \d{4}$/, 
      message: 'Формат: 1234 5678 9012 3456',
      trigger: 'blur'
    }
  ],
  expiry: [
    { required: true, message: 'Введите срок действия', trigger: 'blur' },
    {
      pattern: /^(0[1-9]|1[0-2])\/\d{2}$/, 
      message: 'Формат MM/YY',
      trigger: 'blur'
    }
  ],
  cvv: [
    { required: true, message: 'Введите CVV', trigger: 'blur' },
    { pattern: /^\d{3}$/, message: 'Только 3 цифры', trigger: 'blur' }
  ]
}

const formatCardNumber = () => {
  form.value.number = form.value.number
    .replace(/\D/g, '')
    .replace(/(.{4})/g, '$1 ')
    .trim()
    .slice(0, 19)
}

const formatExpiry = () => {
  let digits = form.value.expiry.replace(/\D/g, '').slice(0, 4)
  if (digits.length >= 3) {
    form.value.expiry = digits.slice(0, 2) + '/' + digits.slice(2)
  } else {
    form.value.expiry = digits
  }
}

onMounted(async () => {
  if (!orderId || !orderNumber.value || totalPrice.value === '...') {
    try {
      const res = await api.get(`/orders/${orderId}`)
      orderStore.setCreatedOrder({
        id: res.data.id,
        number: res.data.number,
        total: res.data.totalPrice
      })
      orderNumber.value = res.data.number
      totalPrice.value = res.data.totalPrice.toFixed(2)
    } catch (err) {
      ElMessage.error('Не удалось загрузить заказ')
    }
  }
  const backPath = router.options.history.state.back
  isFromSummary.value = backPath === '/order/summary'
})

watch(activeTab, async (val) => {
  if (val === 'qr') {
    await nextTick()
    if (qrCanvas.value) {
      const qrText = `https://example.com/pay/${orderId}`
      QRCode.toCanvas(qrCanvas.value, qrText, {
        errorCorrectionLevel: 'H',
        scale: 10,
        margin: 1
      })
    }
  }
})

const submit = async () => {
  if (activeTab.value === 'card') {
    const valid = await formRef.value.validate().catch(() => false)
    if (!valid) return
  }

  try {
    loading.value = true
    await new Promise(resolve => setTimeout(resolve, 1500))
    await api.post(`/orders/${orderId}/pay`)
    ElMessage.success('Оплата прошла успешно!')
    orderStore.resetOrder()
    router.push({ name: 'OrderHistory' })
  } catch (err) {
    ElMessage.error('Ошибка оплаты. Попробуйте позже.')
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.el-input__inner::placeholder {
  color: #bbb;
}
</style>
