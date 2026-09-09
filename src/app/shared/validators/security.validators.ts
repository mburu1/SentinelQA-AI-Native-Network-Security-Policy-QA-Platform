import { AbstractControl, ValidatorFn } from '@angular/forms';

/** IPv4 CIDR validation aligned with the SentinelQA policy engine:
 *  10.0.0.0/24 valid · 192.168.10.5/32 valid · 10.0.0.0/33 invalid · 10.0.0.999/24 invalid
 */
export function isValidIpv4Cidr(value: string): boolean {
  if (!value) return false;
  const parts = value.trim().split('/');
  if (parts.length !== 2) return false;

  const octets = parts[0].split('.');
  if (octets.length !== 4) return false;

  for (const octet of octets) {
    if (!/^\d{1,3}$/.test(octet)) return false;
    const n = Number(octet);
    if (n < 0 || n > 255) return false;
  }

  if (!/^\d{1,2}$/.test(parts[1])) return false;
  const prefix = Number(parts[1]);
  return prefix >= 0 && prefix <= 32;
}

export function cidrValidator(): ValidatorFn {
  return (control: AbstractControl) => {
    if (!control.value) return null;
    return isValidIpv4Cidr(String(control.value)) ? null : { cidr: { value: control.value } };
  };
}

export function portValidator(): ValidatorFn {
  return (control: AbstractControl) => {
    if (control.value === null || control.value === undefined || control.value === '') {
      return { port: { value: control.value } };
    }
    const n = Number(control.value);
    return Number.isInteger(n) && n >= 0 && n <= 65535 ? null : { port: { value: control.value } };
  };
}