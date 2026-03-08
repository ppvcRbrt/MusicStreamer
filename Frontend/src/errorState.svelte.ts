export const errorState = $state<{ hasError: boolean; message: string }>({
    hasError: false,
    message: '',
});

export function setServerError(message: string = 'Something went wrong') {
    errorState.hasError = true;
    errorState.message = message;
}

export function clearServerError() {
    errorState.hasError = false;
    errorState.message = '';
}