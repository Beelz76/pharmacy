export default function formatAddress(address) {
  if (!address) return "";
  const parts = [
    address.region,
    address.city,
    address.suburb,
    address.street,
    address.houseNumber,
    address.postcode,
  ];
  return parts.filter((part) => part && part.trim().length > 0).join(", ");
}
