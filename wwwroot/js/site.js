/* Small JS ripple + playful nudge (uses id exactly "Whatsapp") */
(function () {
	const btn = document.getElementById('Whatsapp');
	if (!btn) return;

	btn.addEventListener('click', function (e) {
		const ripple = document.createElement('span');
		ripple.className = 'ripple';
		const rect = btn.getBoundingClientRect();
		const size = Math.max(rect.width, rect.height) * 0.6;
		ripple.style.width = ripple.style.height = size + 'px';
		const x = (e.clientX || (rect.left + rect.width / 2)) - rect.left - size / 2;
		const y = (e.clientY || (rect.top + rect.height / 2)) - rect.top - size / 2;
		ripple.style.left = x + 'px';
		ripple.style.top = y + 'px';
		btn.appendChild(ripple);
		setTimeout(() => ripple.remove(), 700);
	});

	// gentle nudge after load
	setTimeout(() => {
		if (!btn.matches(':hover')) {
			btn.animate([
				{ transform: 'translateY(0)' },
				{ transform: 'translateY(-10px) rotate(-3deg)' },
				{ transform: 'translateY(0) rotate(0)' }
			], { duration: 650, easing: 'cubic-bezier(.2,.9,.3,1)' });
		}
	}, 1800);
})();