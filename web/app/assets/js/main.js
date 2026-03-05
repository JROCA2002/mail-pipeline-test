(function () {
  "use strict";

  /**
   * Easy selector helper function
   */
  const select = (el, all = false) => {
    el = el.trim()
    if (all) {
      return [...document.querySelectorAll(el)]
    } else {
      return document.querySelector(el)
    }
  }

  /**
   * Easy event listener function
   */
  const on = (type, el, listener, all = false) => {
    let selectEl = select(el, all)
    if (selectEl) {
      if (all) {
        selectEl.forEach(e => e.addEventListener(type, listener))
      } else {
        selectEl.addEventListener(type, listener)
      }
    }
  }

  /**
   * Easy on scroll event listener 
   */
  const onscroll = (el, listener) => {
    el.addEventListener('scroll', listener)
  }

  /**
   * Navbar links active state on scroll
   */
  let navbarlinks = select('#navbar .scrollto', true)
  const navbarlinksActive = () => {
    let position = window.scrollY + 100
    navbarlinks.forEach(navbarlink => {
      if (!navbarlink.hash) return
      let section = select(navbarlink.hash)
      if (!section) return
      if (position >= section.offsetTop && position <= (section.offsetTop + section.offsetHeight)) {
        navbarlink.classList.add('active')
      } else {
        navbarlink.classList.remove('active')
      }
    })
  }
  window.addEventListener('load', navbarlinksActive)
  onscroll(document, navbarlinksActive)

  /**
   * Scrolls to an element with header offset
   */
  const scrollto = (el) => {
    let header = select('#header')
    let offset = header.offsetHeight

    if (!header.classList.contains('header-scrolled')) {
      offset -= 20
    }

    let elementPos = select(el).offsetTop
    window.scrollTo({
      top: elementPos - offset,
      behavior: 'smooth'
    })
  }

  /**
   * Back to top button
   */
  let backtotop = select('.back-to-top')
  if (backtotop) {
    const toggleBacktotop = () => {
      if (window.scrollY > 100) {
        backtotop.classList.add('active')
      } else {
        backtotop.classList.remove('active')
      }
    }
    window.addEventListener('load', toggleBacktotop)
    onscroll(document, toggleBacktotop)
  }

  const navbarCollapse = document.getElementById('navbarNav');
  const header = document.getElementById('header');

  navbarCollapse.addEventListener('show.bs.collapse', () => {
    header.classList.add('menu-open');
  });

  navbarCollapse.addEventListener('hide.bs.collapse', () => {
    header.classList.remove('menu-open');
  });

  // CERRAR AL CLICK EN LINKS (mobile)
  document.querySelectorAll('#navbarNav .nav-link').forEach(link => {
    link.addEventListener('click', () => {
      const bsCollapse = bootstrap.Collapse.getInstance(navbarCollapse);
      if (bsCollapse) {
        bsCollapse.hide();
      }
    });
  });
 })()

const form = document.querySelector(".php-email-form");
const successMessage = document.getElementById('success-message');
const errorMessage = document.getElementById('error-message');

function showTemporaryMessage(element, duration = 5000) {
  [successMessage, errorMessage].forEach(msg => {
    msg.classList.remove('show-message');
    msg.style.display = 'none';
  });

  element.style.display = 'block';
  setTimeout(() => {
    element.classList.add('show-message');
  }, 10);

  setTimeout(() => {
    element.classList.remove('show-message');
    setTimeout(() => {
      element.style.display = 'none';
    }, 500);
  }, duration);
}

function isEmpty(value) {
  return value == null || String(value).trim() === '';
}

function validarTelefono(telefono) {
  return telefono.length >= 8 && telefono.length <= 15 && /^\d+$/.test(telefono);
}

if (form) {
  form.addEventListener("submit", function (event) {
    event.preventDefault();

    // Obtengo el token que genera Google al marcar "No soy un robot"
    const captchaToken = grecaptcha.getResponse();

    if (!captchaToken) {
      errorMessage.textContent = "Por favor, completa el reCAPTCHA antes de enviar.";
      showTemporaryMessage(errorMessage);
      return;
    }

    const submitButton = form.querySelector('.btn-primary');
    submitButton.disabled = true;

    const data = {
      Token: captchaToken,
      Nombre: form.nombre.value?.trim(),
      Empresa: form.empresa.value?.trim(),
      Email: form.email.value?.trim(),
      Telefono: form.telefono.value?.trim(),
      Mensaje: form.mensaje.value?.trim()
    };

    const emptyFields = [];

    for (const key in data) {
      if (key != "Email" && isEmpty(data[key])) {
        emptyFields.push(key);
      }
    }

    if(emptyFields.length > 0) {
      errorMessage.textContent = "Por favor, completa todos los campos obligatorios.";
      showTemporaryMessage(errorMessage);
      submitButton.disabled = false;
      return;
    }

    if (!validarTelefono(data.Telefono)) {
      errorMessage.textContent = "Por favor, ingresa un número de teléfono válido.";
      showTemporaryMessage(errorMessage);
      submitButton.disabled = false;
      return;
    }

    fetch("/api/EnvioMail", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(data)
    })
      .then(response => response.json())
      .then(result => {
        console.log(result); // TODO : Eliminar este console.log, es solo para pruebas
        if (result.ok) {
          successMessage.textContent = "¡Mensaje enviado correctamente! Te contactaremos pronto.";
          showTemporaryMessage(successMessage);
          form.reset();
          grecaptcha.reset();
        } else {
          errorMessage.textContent = "Captcha inválido o error al enviar el correo.";
          showTemporaryMessage(errorMessage);
          grecaptcha.reset();
        }
      })
      .catch(error => {
        console.error("Error al procesar el formulario:", error); // TODO : Eliminar este console.log, es solo para pruebas
        errorMessage.textContent = "Ocurrió un error de conexión al enviar el formulario.";
        showTemporaryMessage(errorMessage);
      })
      .finally(() => {
        submitButton.disabled = false;
      });
  });

  const cards = document.querySelectorAll('.service-card');

  const observer = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
      if (entry.isIntersecting) {
        entry.target.classList.add('show');
      }
    });
  }, { threshold: 0.2 }); // se activa cuando el 20% del elemento es visible

  cards.forEach((card, index) => {
    // Alterna animaciones izquierda / derecha
    if (index % 2 === 0) {
      card.classList.add('from-left');
    } else {
      card.classList.add('from-right');
    }
    observer.observe(card);
  });

  const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]')
  const tooltipList = [...tooltipTriggerList].map(tooltipTriggerEl => new bootstrap.Tooltip(tooltipTriggerEl))

  setTimeout(() => {
    fetch("/api/load_config", {
        method: 'GET', // Explicitly specifying GET (optional, since it's default)
        headers: {
            'Accept': 'application/json'
        }
    })
    .then(response => {
        // Check if the response is OK (status 200–299)
        if (!response.ok) {
            throw new Error(`HTTP error! Status: ${response.status}`);
        }
        return response.json(); // Parse JSON response
    })
    .then(data => {
      const siteKey = data.captcha_options.GoogleToken;
      if (!siteKey) {
        throw new Error("Captcha key not found in response");
      }
      // Render reCAPTCHA dynamically
      grecaptcha.render("recaptcha-container", {
        sitekey: siteKey
      });
        console.log('Fetched data:', data); // TODO : Eliminar este console.log, es solo para pruebas
    })
    .catch(error => {
        console.error('Fetch error:', error.message); // TODO : Eliminar este console.error, es solo para pruebas
    });
  }, 2000); // Esperar 2 segundos antes de hacer la solicitud

    // TODO : Este metodo es de prueba, luego BORRAR
    setTimeout(() => {
    fetch("/api/TestIntegration", {
        method: 'GET', // Explicitly specifying GET (optional, since it's default)
        headers: {
            'Accept': 'application/json'
        }
    })
    .then(response => {
        // Check if the response is OK (status 200–299)
        if (!response.ok) {
            throw new Error(`HTTP error! Status: ${response.status}`);
        }
        return response.json(); // Parse JSON response
    })
    .then(data => {
        console.log('Fetched data de Azure Function MGraph:', data); // TODO : Eliminar este console.log, es solo para pruebas
    })
    .catch(error => {
        console.error('Fetch error:', error.message); // TODO : Eliminar este console.error, es solo para pruebas
    });
  }, 2000); // Esperar 2 segundos antes de hacer la solicitud

}
