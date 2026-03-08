import { Haptics, ImpactStyle } from '@capacitor/haptics';
import { isNativePlatform } from '$lib/utils/platform';

export async function hapticLight() {
    if (!isNativePlatform) return;
    await Haptics.impact({ style: ImpactStyle.Light });
}

export async function hapticMedium() {
    if (!isNativePlatform) return;
    await Haptics.impact({ style: ImpactStyle.Medium });
}

export async function hapticHeavy() {
    if (!isNativePlatform) return;
    await Haptics.impact({ style: ImpactStyle.Heavy });
}

export async function hapticSelection() {
    if (!isNativePlatform) return;
    await Haptics.selectionChanged();
}
