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
      <el-form
        label-width="150px"
        :model="formModel"
        :rules="allRules"
        ref="productFormRef"
      >
        <el-form-item label="Название" prop="name">
          <el-input v-model="productForm.name" />
        </el-form-item>
        <el-form-item label="Описание" prop="description">
          <el-input v-model="productForm.description" />
        </el-form-item>
        <el-form-item
          label="Расшир. описание"
          prop="extendedDescription"
          :required="false"
        >
          <el-input v-model="productForm.extendedDescription" type="textarea" />
        </el-form-item>
        <el-form-item label="Категория" prop="categoryId">
          <el-select v-model="productForm.categoryId" class="!w-60" filterable>
            <el-option
              v-for="c in flatCategories"
              :key="c.id"
              :value="c.id"
              :label="c.name"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="Производитель" prop="manufacturerId">
          <el-select
            v-model="productForm.manufacturerId"
            class="!w-60"
            filterable
          >
            <el-option
              v-for="m in manufacturers"
              :key="m.id"
              :value="m.id"
              :label="m.name"
            />
          </el-select>
        </el-form-item>
        <el-form-item label="Цена" prop="price">
          <el-input type="number" v-model.number="productForm.price" />
        </el-form-item>
        <el-form-item label="Доступен" prop="isAvailable" :required="false">
          <el-switch v-model="productForm.isAvailable" />
        </el-form-item>

        <template v-for="field in categoryFields" :key="field.key">
          <el-form-item :label="field.label" :prop="field.key">
            <el-date-picker
              v-if="field.type === 'date'"
              v-model="propertyValues[field.key]"
              type="date"
            />
            <el-switch
              v-else-if="field.type === 'boolean'"
              v-model="propertyValues[field.key]"
            />
            <el-input
              v-else
              v-model="propertyValues[field.key]"
              :type="
                field.type === 'number' || field.type === 'integer'
                  ? 'number'
                  : 'text'
              "
            />
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
import { ref, reactive, onMounted, watch, computed } from "vue";
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
  categoryId: null,
  manufacturerId: null,
  isAvailable: true,
  properties: [],
});

const categories = ref([]);
const flatCategories = ref([]);
const manufacturers = ref([]);
const categoryFields = ref([]);
const propertyValues = reactive({});
const formModel = computed(() => ({ ...productForm, ...propertyValues }));

const images = ref([]);
const pendingFiles = ref([]);
const pendingExternalUrls = ref([]);
const newImageUrl = ref("");
const imagesToDelete = ref([]);
const loading = ref(false);
const productFormRef = ref();
const fieldRules = reactive({});

const rules = {
  name: [{ required: true, message: "Введите название", trigger: "blur" }],
  price: [{ required: true, message: "Введите цену", trigger: "blur" }],
  categoryId: [
    { required: true, message: "Выберите категорию", trigger: "change" },
  ],
  manufacturerId: [
    { required: true, message: "Выберите производителя", trigger: "change" },
  ],
  description: [
    { required: true, message: "Введите описание", trigger: "blur" },
  ],
};

const allRules = computed(() => ({ ...rules, ...fieldRules }));

function flatten(list, prefix = "", arr = []) {
  for (const c of list) {
    arr.push({ id: c.id, name: prefix + c.name });
    if (c.subcategories?.length) {
      flatten(c.subcategories, prefix + c.name + " / ", arr);
    }
  }
  return arr;
}

onMounted(async () => {
  categories.value = await getAllCategories();
  flatCategories.value = flatten(categories.value);
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
        categoryId: data.category.id,
        manufacturerId: data.manufacturer.id,
        isAvailable: data.isAvailable,
        properties: data.properties || [],
      });
      images.value = data.images || [];
    } finally {
      loading.value = false;
    }
  }
});

async function loadCategoryFields(id) {
  const fields = await getCategoryFields(id);
  categoryFields.value = fields;
  Object.keys(fieldRules).forEach((k) => delete fieldRules[k]);
  fields.forEach((f) => {
    const existing = productForm.properties.find((p) => p.key === f.key);
    propertyValues[f.key] = existing ? existing.value : "";
    const rulesArr = [];
    if (f.isRequired) {
      rulesArr.push({
        required: true,
        message: "Обязательное поле",
        trigger: "blur",
      });
    }
    if (f.type === "number") {
      rulesArr.push({
        validator: (_, val, cb) =>
          isNaN(val) ? cb(new Error("Введите число")) : cb(),
        trigger: "blur",
      });
    }
    if (f.type === "integer") {
      rulesArr.push({
        validator: (_, val, cb) =>
          !Number.isInteger(+val) ? cb(new Error("Введите целое число")) : cb(),
        trigger: "blur",
      });
    }
    fieldRules[f.key] = rulesArr;
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
  const files = Array.from(e.target.files);
  for (const file of files) {
    const id = Date.now() + Math.random();
    pendingFiles.value.push({ id, file });
    images.value.push({ id, url: URL.createObjectURL(file), tmp: true });
  }
  e.target.value = null;
}

async function addImageLink() {
  if (!newImageUrl.value) return;
  images.value.push({
    id: Date.now() + Math.random(),
    url: newImageUrl.value,
    tmp: true,
  });
  pendingExternalUrls.value.push(newImageUrl.value);
  newImageUrl.value = "";
}

async function removeImage(img) {
  const pendingIdx = pendingExternalUrls.value.indexOf(img.url);
  if (pendingIdx !== -1) {
    pendingExternalUrls.value.splice(pendingIdx, 1);
    images.value = images.value.filter((i) => i !== img);
    return;
  }
  if (img.tmp) {
    pendingFiles.value = pendingFiles.value.filter((f) => f.id !== img.id);
  } else {
    imagesToDelete.value.push(img.id);
  }
  images.value = images.value.filter((i) => i !== img);
}

async function save() {
  const valid = await productFormRef.value.validate().catch(() => false);
  if (!valid) return;
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
      value: String(propertyValues[f.key] ?? ""),
    })),
  };
  try {
    let id = productForm.id;
    if (isNew) {
      const res = await ProductService.create(payload);
      id = res.id;
      productForm.id = id;
      ElMessage.success("Создано");
    } else {
      await ProductService.update(productForm.id, payload);
      ElMessage.success("Сохранено");
    }
    if (pendingFiles.value.length) {
      try {
        const files = pendingFiles.value.map((f) => f.file);
        const res = await uploadProductImages(id, files);
        images.value = images.value.filter((i) => !i.tmp);
        images.value.push(...res.images);
        pendingFiles.value = [];
      } catch {}
    }

    if (pendingExternalUrls.value.length) {
      try {
        const res = await addExternalImages(id, pendingExternalUrls.value);
        images.value = images.value.filter((i) => !i.tmp);
        images.value.push(...res.images);
        pendingExternalUrls.value = [];
      } catch {}
    }

    if (imagesToDelete.value.length) {
      try {
        await deleteProductImages(id, imagesToDelete.value);
        imagesToDelete.value = [];
      } catch {}
    }
    router.replace({ name: "AdminProducts", query: route.query });
  } catch {}
}
</script>
