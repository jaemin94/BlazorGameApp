// wwwroot/gameInput.js
// 방향키, 공격, 대쉬, 스킬, 창 열기 키 입력 처리
window.gameInput = {
    init: function (dotNetRef) {
        if (window.gameInputInitialized) {
            return;
        }

        window.gameInputInitialized = true;

        document.addEventListener("keydown", function (e) {
            const key = e.key;
            const code = e.code;

            if (code === "ControlLeft" || code === "ControlRight") {
                e.preventDefault();
                dotNetRef.invokeMethodAsync("OnKeyDown", "Control");
                return;
            }

            if (code === "ShiftLeft" || code === "ShiftRight") {
                e.preventDefault();
                dotNetRef.invokeMethodAsync("OnKeyDown", "Shift");
                return;
            }

            const allowedKeys = [
                "1", "2", "3", "4", "5", "6", "7", "8", "9",
                "i", "I", "k", "K", "s", "S", "e", "E", "z", "Z",
                "ArrowUp", "ArrowDown", "ArrowLeft", "ArrowRight"
            ];

            if (allowedKeys.includes(key)) {
                e.preventDefault();
                dotNetRef.invokeMethodAsync("OnKeyDown", key);
            }
        });
    }
};
