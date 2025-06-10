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
        {{ isNew ? "Новый товар" : `Товар #${productForm.id}` }}
      </h2>
    </div>

    <div v-if="loading" class="text-center py-10">Загрузка...</div>

    <div v-else class="bg-white border rounded-xl p-6 shadow-sm space-y-4">
      <el-form label-width="150px">
        <el-form-item label="Название">
          <el-input v-model="productForm.name" />
        </el-form-item>
        <el-form-item label="Описание">
          <el-input v-model="productForm.description" />
        </el-form-item>
        <el-form-item label="Расшир. описание">
          <el-input v-model="productForm.extendedDescription" type="textarea" />
        </el-form-item>
        <el-form-item label="Категория">
          <el-select v-model="productForm.categoryId" class="!w-60">
            <el-option
              v-for="c in categories"
              :key="c.id"
              :value="c.id"
              :label="c.name"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="Производитель">
          <el-select v-model="productForm.manufacturerId" class="!w-60">
            <el-option
              v-for="m in manufacturers"
              :key="m.id"
              :value="m.id"
              :label="m.name"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="Цена">
          <el-input type="number" v-model.number="productForm.price" />
        </el-form-item>
        <el-form-item label="Количество">
          <el-input type="number" v-model.number="productForm.stockQuantity" />
        </el-form-item>
        <el-form-item label="Доступен">
          <el-switch v-model="productForm.isAvailable" />
        </el-form-item>

        <template v-for="field in categoryFields" :key="field.key">
          <el-form-item :label="field.label">
            <el-input v-model="propertyValues[field.key]" />
          </el-form-item>
        </template>
      </el-form>

      <div>
        <h3 class="font-semibold mb-2">Изображения</h3>
        <div class="flex flex-wrap gap-2 mb-2">
          <div v-for="img in images" :key="img.id" class="relative">
            <img
              :src="img.url"
              class="w-24 h-24 object-cover rounded-md border"
            />
            <button
              class="absolute top-0 right-0 bg-white/80 text-red-600 p-1 rounded-bl-md"
              @click.prevent="removeImage(img)"
            >
              <i class="fas fa-times"></i>
            </button>
          </div>
        </div>
        <input type="file" multiple @change="onFileChange" />
        <el-button
          class="mt-2"
          @click="uploadFiles"
          :disabled="selectedFiles.length === 0"
          >Загрузить</el-button
        >
        <div class="mt-4 flex items-center gap-2">
          <el-input
            v-model="newImageUrl"
            placeholder="Ссылка на изображение"
            class="!w-72"
          />
          <el-button @click="addImageLink" :disabled="!newImageUrl"
            >Добавить</el-button
          >
        </div>
      </div>

      <div class="text-right pt-4">
        <el-button type="primary" @click="save">Сохранить</el-button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, watch } from "vue";
import { useRoute, useRouter } from "vue-router";
import {
  getAllCategories,
  getCategoryFields,
} from "/src/services/CategoryService";
import { getManufacturers } from "/src/services/ManufacturerService";
import ProductService from "/src/services/ProductService";
import {
  uploadProductImages,
  deleteProductImages,
  addExternalImages,
} from "/src/services/ProductImageService";
import { ElMessage } from "element-plus";

const route = useRoute();
const router = useRouter();

const isNew = route.name === "AdminProductCreate";
const productId = isNew ? null : Number(route.params.id);

const productForm = reactive({
  id: productId,
  name: "",
  description: "",
  extendedDescription: "",
  price: 0,
  stockQuantity: 0,
  categoryId: null,
  manufacturerId: null,
  isAvailable: true,
  properties: [],
});

const categories = ref([]);
const manufacturers = ref([]);
const categoryFields = ref([]);
const propertyValues = reactive({});

const images = ref([]);
const selectedFiles = ref([]);
const newImageUrl = ref("");
const loading = ref(false);

onMounted(async () => {
  categories.value = await getAllCategories();
  manufacturers.value = await getManufacturers();
  if (!isNew) {
    loading.value = true;
    try {
      const data = await ProductService.getById(productId);
      Object.assign(productForm, {
        id: data.id,
        name: data.name,
        description: data.description,
        extendedDescription: data.extendedDescription,
        price: data.price,
        stockQuantity: data.stockQuantity,
        categoryId: data.category.id,
        manufacturerId: data.manufacturer.id,
        isAvailable: data.isAvailable,
        properties: data.properties || [],
      });
      images.value = (data.images || []).map((url, idx) => ({ id: idx, url }));
    } finally {
      loading.value = false;
    }
  }
  if (productForm.categoryId) {
    await loadCategoryFields(productForm.categoryId);
  }
});

async function loadCategoryFields(id) {
  const fields = await getCategoryFields(id);
  categoryFields.value = fields;
  fields.forEach((f) => {
    const existing = productForm.properties.find((p) => p.key === f.key);
    propertyValues[f.key] = existing ? existing.value : "";
  });
}

watch(
  () => productForm.categoryId,
  async (val) => {
    if (val) {
      await loadCategoryFields(val);
    }
  }
);

function onFileChange(e) {
  selectedFiles.value = Array.from(e.target.files);
}

async function uploadFiles() {
  if (!selectedFiles.value.length || !productForm.id) return;
  try {
    const res = await uploadProductImages(productForm.id, selectedFiles.value);
    images.value.push(
      ...res.urls.map((url) => ({ id: Date.now() + Math.random(), url }))
    );
    selectedFiles.value = [];
    ElMessage.success("Загружено");
  } catch (e) {
    ElMessage.error(e.message);
  }
}

async function addImageLink() {
  if (!newImageUrl.value) return;
  if (productForm.id) {
    try {
      await addExternalImages(productForm.id, [newImageUrl.value]);
      images.value.push({
        id: Date.now() + Math.random(),
        url: newImageUrl.value,
      });
      newImageUrl.value = "";
    } catch (e) {
      ElMessage.error(e.message);
    }
  } else {
    images.value.push({
      id: Date.now() + Math.random(),
      url: newImageUrl.value,
    });
    newImageUrl.value = "";
  }
}

async function removeImage(img) {
  if (!productForm.id) {
    images.value = images.value.filter((i) => i !== img);
    return;
  }
  try {
    await deleteProductImages(productForm.id, [img.id]);
    images.value = images.value.filter((i) => i !== img);
  } catch {}
}

async function save() {
  const payload = {
    name: productForm.name,
    price: productForm.price,
    stockQuantity: productForm.stockQuantity,
    categoryId: productForm.categoryId,
    manufacturerId: productForm.manufacturerId,
    description: productForm.description,
    extendedDescription: productForm.extendedDescription,
    isAvailable: productForm.isAvailable,
    properties: categoryFields.value.map((f) => ({
      key: f.key,
      label: f.label,
      value: propertyValues[f.key] || "",
    })),
  };
  try {
    if (isNew) {
      const res = await ProductService.create(payload);
      ElMessage.success("Создано");
      router.replace({ name: "AdminProductDetails", params: { id: res.id } });
    } else {
      await ProductService.update(productForm.id, payload);
      ElMessage.success("Сохранено");
    }
  } catch (e) {
    ElMessage.error(
      `${e.response?.status} ${e.response?.data?.message || e.message}`
    );
  }
}
</script>
