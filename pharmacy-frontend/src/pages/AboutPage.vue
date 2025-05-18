<template>
  <div class="max-w-6xl mx-auto py-16 px-4">
    <!-- Заголовок -->
    <h1 class="text-4xl font-extrabold text-gray-900 mb-8 text-center">О нашей интернет-аптеке</h1>

    <p class="text-lg text-gray-700 max-w-3xl mx-auto text-center mb-12">
      Мы — современная интернет-аптека, предоставляющая удобный способ заказать необходимые лекарства онлайн и забрать их в ближайшей аптеке. Надёжно. Быстро. Комфортно.
    </p>

    <!-- Преимущества -->
    <div class="grid md:grid-cols-3 gap-8 mb-16">
      <div class="p-6 border rounded-xl text-center bg-white shadow hover:shadow-md transition">
        <i class="fas fa-capsules text-primary-600 text-4xl mb-4"></i>
        <h3 class="text-lg font-semibold mb-2">Большой выбор</h3>
        <p class="text-gray-600">Тысячи препаратов, витаминов и товаров для здоровья.</p>
      </div>
      <div class="p-6 border rounded-xl text-center bg-white shadow hover:shadow-md transition">
        <i class="fas fa-shield-alt text-primary-600 text-4xl mb-4"></i>
        <h3 class="text-lg font-semibold mb-2">Качество и безопасность</h3>
        <p class="text-gray-600">Сертифицированная продукция от проверенных поставщиков.</p>
      </div>
      <div class="p-6 border rounded-xl text-center bg-white shadow hover:shadow-md transition">
        <i class="fas fa-store-alt text-primary-600 text-4xl mb-4"></i>
        <h3 class="text-lg font-semibold mb-2">Удобный самовывоз</h3>
        <p class="text-gray-600">Заберите заказ в удобное время без ожидания.</p>
      </div>
    </div>

    <!-- Забота -->
    <div class="bg-primary-50 rounded-xl p-8 text-center mb-20 shadow-sm">
      <h2 class="text-2xl font-bold mb-3">Мы заботимся о вашем удобстве</h2>
      <p class="text-gray-700 text-base max-w-3xl mx-auto">
        Наш приоритет — ваше здоровье. Мы улучшаем сервис каждый день, чтобы покупка лекарств стала проще, быстрее и безопаснее.
      </p>
    </div>

    <!-- Пошаговая инструкция -->
    <div class="mb-20">
      <h2 class="text-3xl font-bold mb-12 text-center">Как оформить заказ</h2>
      <div class="grid md:grid-cols-2 gap-6">
        <div
          v-for="(step, index) in steps"
          :key="index"
          class="bg-white shadow rounded-xl p-6 flex gap-5"
        >
          <div class="w-10 h-10 bg-primary-600 text-white font-bold rounded-full flex items-center justify-center text-lg">
            {{ index + 1 }}
          </div>
          <div>
            <h3 class="text-lg font-semibold mb-1">{{ step.title }}</h3>
            <p class="text-gray-600 text-sm">{{ step.description }}</p>
          </div>
        </div>
      </div>
    </div>

    <!-- Контакты -->
    <div class="mt-10">
      <h2 class="text-3xl font-bold text-center mb-8">Связаться с нами</h2>
      <div class="grid sm:grid-cols-2 gap-8 max-w-2xl mx-auto text-gray-700">
        <div class="flex items-start gap-4">
          <i class="fas fa-envelope text-primary-600 text-xl mt-1"></i>
          <div>
            <p class="font-medium">Email</p>
            <p>support@medicare.ru</p>
          </div>
        </div>
        <div class="flex items-start gap-4">
          <i class="fas fa-phone text-primary-600 text-xl mt-1"></i>
          <div>
            <p class="font-medium">Телефон</p>
            <p>+7 (495) 123-45-67</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
const steps = [
  {
    title: 'Выберите нужные товары',
    description: 'Найдите нужный препарат через поиск или каталог и добавьте его в корзину.',
  },
  {
    title: 'Перейдите к оформлению',
    description: 'Выберите город, аптеку и подходящий способ оплаты.',
  },
  {
    title: 'Подтвердите заказ',
    description: 'Проверьте введённые данные и подтвердите оформление заказа.',
  },
  {
    title: 'Заберите заказ в аптеке',
    description: 'Придите в выбранную аптеку и получите заказ без ожидания.',
  },
]
</script>