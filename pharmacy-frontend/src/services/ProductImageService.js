import api from "../utils/axios";

export async function uploadProductImages(productId, files) {
  const formData = new FormData();
  files.forEach((f) => formData.append("files", f));
  const res = await api.post(`/products/${productId}/upload-images`, formData, {
    headers: { "Content-Type": "multipart/form-data" },
  });
  return res.data;
}

export async function addExternalImages(productId, urls) {
  const res = await api.post(`/products/${productId}/add-external-images`, {
    urls,
  });
  return res.data;
}

export async function deleteProductImages(productId, imageIds) {
  await api.delete(`/products/${productId}/images`, { data: { imageIds } });
}
