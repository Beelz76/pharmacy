export function statusClass(status) {
  switch (status) {
    case 'Ожидает оплаты':
    case 'Ожидает обработки':
      return 'bg-yellow-100 text-yellow-800'
    case 'В обработке':
    case 'Готов к получению':
      return 'bg-blue-100 text-blue-800'
    case 'Получен':
      return 'bg-green-100 text-green-700'
    case 'Отменен':
    case 'Возврат средств':
      return 'bg-red-100 text-red-700'
    default:
      return 'bg-gray-100 text-gray-600'
  }
}
