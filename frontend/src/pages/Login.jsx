import { useState } from 'react';
import { AlertCircle, Boxes, CheckCircle2 } from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import { Button } from '../components/ui/button';
import { Input } from '../components/ui/input';
import { Label } from '../components/ui/label';
import { Tabs, TabsList, TabsTrigger } from '../components/ui/tabs';
import { Alert, AlertDescription, AlertTitle } from '../components/ui/alert';

function Login() {
  const [isLogin, setIsLogin] = useState(true);
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [loading, setLoading] = useState(false);
  const { login, register } = useAuth();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setSuccess('');
    setLoading(true);

    if (isLogin) {
      const result = await login(email, password);
      setLoading(false);
      if (result.success) {
        window.location.href = '/products';
      } else {
        setError(result.error);
      }
      return;
    }

    const result = await register({
      firstName: firstName.trim(),
      lastName: lastName.trim(),
      email: email.trim(),
      password,
    });
    setLoading(false);

    if (result.success) {
      setSuccess(result.message || 'Usuario registrado exitosamente. Ahora puedes iniciar sesión.');
      setFirstName('');
      setLastName('');
      setEmail('');
      setPassword('');
      setTimeout(() => {
        setIsLogin(true);
        setSuccess('');
      }, 2000);
    } else {
      setError(result.error);
    }
  };

  const handleDemo = () => {
    setEmail('cri.verap@duocuc.cl');
    setPassword('Iceland12.');
    setIsLogin(true);
    setError('');
    setSuccess('');
  };

  return (
    <div className="grid min-h-screen lg:grid-cols-2">

      {/* Columna izquierda: solo visible en desktop */}
      <div className="hidden bg-primary/5 lg:flex lg:flex-col lg:justify-between lg:p-12">
        <div className="flex items-center gap-2">
          <Boxes className="h-6 w-6 text-primary" />
          <span className="text-lg font-semibold">WareFlow</span>
        </div>

        <div className="space-y-4">
          <h1 className="text-3xl font-semibold tracking-tight">
            Inventario y órdenes, sin planillas
          </h1>
          <p className="text-muted-foreground">
            Gestiona productos, controla stock y procesa órdenes desde un solo lugar.
          </p>
          <ul className="space-y-2 text-sm text-muted-foreground">
            <li className="flex items-center gap-2">
              <CheckCircle2 className="h-4 w-4 text-primary" />
              Stock en tiempo real
            </li>
            <li className="flex items-center gap-2">
              <CheckCircle2 className="h-4 w-4 text-primary" />
              Descuento automático al vender
            </li>
            <li className="flex items-center gap-2">
              <CheckCircle2 className="h-4 w-4 text-primary" />
              Roles para admin, vendedor y bodeguero
            </li>
          </ul>
        </div>

        <p className="text-xs text-muted-foreground">
          © {new Date().getFullYear()} WareFlow
        </p>
      </div>

      {/* Columna derecha: formulario */}
      <div className="flex items-center justify-center p-6">
        <div className="w-full max-w-sm space-y-6">
          <div className="flex items-center justify-center gap-2 lg:hidden">
            <Boxes className="h-6 w-6 text-primary" />
            <span className="text-lg font-semibold">WareFlow</span>
          </div>

          <div className="space-y-1 text-center lg:text-left">
            <h2 className="text-2xl font-semibold tracking-tight">
              {isLogin ? 'Iniciar sesión' : 'Crear cuenta'}
            </h2>
            <p className="text-sm text-muted-foreground">
              {isLogin
                ? 'Ingresa tus credenciales para continuar'
                : 'Completa el formulario para registrarte'}
            </p>
          </div>

          <Tabs
            value={isLogin ? 'login' : 'register'}
            onValueChange={(value) => {
              setIsLogin(value === 'login');
              setError('');
              setSuccess('');
            }}
          >
            <TabsList className="grid w-full grid-cols-2">
              <TabsTrigger value="login" disabled={loading}>
                Iniciar sesión
              </TabsTrigger>
              <TabsTrigger value="register" disabled={loading}>
                Registrarse
              </TabsTrigger>
            </TabsList>
          </Tabs>

          {error && (
            <Alert variant="destructive">
              <AlertCircle className="h-4 w-4" />
              <AlertTitle>Error</AlertTitle>
              <AlertDescription>{error}</AlertDescription>
            </Alert>
          )}

          {success && (
            <Alert>
              <CheckCircle2 className="h-4 w-4 text-success" />
              <AlertTitle>Éxito</AlertTitle>
              <AlertDescription>{success}</AlertDescription>
            </Alert>
          )}

          <form onSubmit={handleSubmit} className="space-y-4">
            {!isLogin && (
              <div className="grid grid-cols-2 gap-3">
                <div className="space-y-1.5">
                  <Label htmlFor="firstName">Nombre</Label>
                  <Input
                    id="firstName"
                    type="text"
                    autoComplete="given-name"
                    value={firstName}
                    onChange={(e) => setFirstName(e.target.value)}
                    disabled={loading}
                    required
                  />
                </div>
                <div className="space-y-1.5">
                  <Label htmlFor="lastName">Apellido</Label>
                  <Input
                    id="lastName"
                    type="text"
                    autoComplete="family-name"
                    value={lastName}
                    onChange={(e) => setLastName(e.target.value)}
                    disabled={loading}
                    required
                  />
                </div>
              </div>
            )}

            <div className="space-y-1.5">
              <Label htmlFor="email">Email</Label>
              <Input
                id="email"
                type="email"
                autoComplete="email"
                placeholder="tu@correo.cl"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                disabled={loading}
                required
              />
            </div>

            <div className="space-y-1.5">
              <Label htmlFor="password">Contraseña</Label>
              <Input
                id="password"
                type="password"
                autoComplete={isLogin ? 'current-password' : 'new-password'}
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                disabled={loading}
                required
              />
            </div>

            <Button type="submit" className="w-full" disabled={loading}>
              {loading ? 'Cargando...' : isLogin ? 'Iniciar sesión' : 'Crear cuenta'}
            </Button>
          </form>

          <div className="relative">
            <div className="absolute inset-0 flex items-center">
              <span className="w-full border-t" />
            </div>
            <div className="relative flex justify-center text-xs uppercase">
              <span className="bg-background px-2 text-muted-foreground">o</span>
            </div>
          </div>

          <Button
            type="button"
            variant="outline"
            className="w-full"
            onClick={handleDemo}
            disabled={loading}
          >
            Entrar como demo
          </Button>
        </div>
      </div>
    </div>
  );
}

export default Login;
