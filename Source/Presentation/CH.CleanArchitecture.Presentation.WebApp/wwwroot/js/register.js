var strength = {
    0: "Worst",
    1: "Bad",
    2: "Weak",
    3: "Good",
    4: "Strong"
}

var password = document.getElementById('frm-reg-pass');
var meter = document.getElementById('password-strength-meter');
var text = document.getElementById('password-strength-text');

password.addEventListener('input', function () {
    var val = password.value;
    var result = zxcvbn(val);

    // Update the password strength meter
    meter.value = result.score;

    // Update the text indicator
    if (val !== "") {
        text.innerHTML = "Strength: " + strength[result.score];
    } else {
        text.innerHTML = "";
    }
});