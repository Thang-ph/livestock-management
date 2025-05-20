import { Button } from '@/components/ui/button';
import {
  Leaf,
  BarChart3,
  Calendar,
  CloudSun,
  Tractor,
  Users,
  ChevronRight,
  Check,
  ArrowRight
} from 'lucide-react';
import Banner from '@/assets/banner.jpeg';
import Logo1 from '@/assets/logo1.jpg';
import Logo2 from '@/assets/logo2.png';
import Logo3 from '@/assets/logo3.jpg';
import Logo4 from '@/assets/logo4.png';
import Logo5 from '@/assets/logo5.png';
import Logo6 from '@/assets/logo6.jpg';
import { useRouter } from '@/routes/hooks';

const logos = [Logo1, Logo2, Logo3, Logo4, Logo5, Logo6];

export default function LandingPage() {
  const router = useRouter();
  return (
    <div className="flex min-h-screen flex-col">
      <header className="sticky top-0 z-50 w-full border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
        <div className="container flex h-16 items-center justify-between">
          <div className="flex items-center gap-2">
            <Leaf className="h-6 w-6 text-green-600" />
            <span className="text-xl font-bold">LMS</span>
          </div>
          <nav className="hidden gap-6 md:flex">
            <a
              href="#features"
              className="text-sm font-medium transition-colors hover:text-green-600"
            >
              Tính năng
            </a>
            <a
              href="#how-it-works"
              className="text-sm font-medium transition-colors hover:text-green-600"
            >
              Cách thức hoạt động
            </a>
            <a
              href="#benefits"
              className="text-sm font-medium transition-colors hover:text-green-600"
            >
              Lợi ích
            </a>
            <a
              href="#testimonials"
              className="text-sm font-medium transition-colors hover:text-green-600"
            >
              Đánh giá
            </a>
          </nav>
          <div className="flex items-center gap-4">
            <a
              href="#contact"
              className="hidden h-9 items-center justify-center rounded-md border border-input bg-background px-4 py-2 text-sm font-medium shadow-sm transition-colors hover:bg-accent hover:text-accent-foreground md:inline-flex"
            >
              Liên hệ
            </a>
            <Button className="bg-green-600 hover:bg-green-700">
              Tìm hiểu thêm
            </Button>
          </div>
        </div>
      </header>
      <main className="flex-1">
        {/* Hero Section */}
        <section className="relative">
          <div className="absolute inset-0 z-0 bg-gradient-to-b from-green-50 to-white"></div>
          <div className="container relative z-10 py-20 md:py-32">
            <div className="grid items-center gap-8 md:grid-cols-2">
              <div className="space-y-6">
                <h1 className="text-4xl font-bold leading-tight tracking-tighter md:text-5xl lg:text-6xl">
                  Quản lý trang trại thông minh cho năng suất tối ưu
                </h1>
                <p className="max-w-[600px] text-lg text-muted-foreground md:text-xl">
                  Hệ thống quản lý trang trại toàn diện giúp bạn theo dõi, phân
                  tích và tối ưu hóa mọi khía cạnh của hoạt động nông nghiệp.
                </p>
                <div className="flex flex-col gap-4 sm:flex-row">
                  <Button size="lg" className="bg-green-600 hover:bg-green-700">
                    Tìm hiểu thêm
                    <ChevronRight className="ml-2 h-4 w-4" />
                  </Button>
                  <Button
                    variant="outline"
                    size="lg"
                    className="border-green-600 text-green-600 hover:bg-green-100"
                    onClick={() => router.push('/login')}
                  >
                    Trang quản lý
                  </Button>
                </div>
                <div className="flex items-center gap-4 text-sm text-muted-foreground">
                  <div className="flex -space-x-2">
                    {[1, 2, 3, 4].map((i) => (
                      <div
                        key={i}
                        className="flex h-8 w-8 items-center justify-center rounded-full border-2 border-background bg-green-100"
                      >
                        <span className="font-medium text-green-700">{i}</span>
                      </div>
                    ))}
                  </div>
                  <div>Hơn 1,000+ nông trại đang sử dụng</div>
                </div>
              </div>
              <div className="relative">
                <div className="absolute -inset-1 rounded-xl bg-gradient-to-r from-green-400 to-emerald-500 opacity-30 blur-xl"></div>
                <div className="relative overflow-hidden rounded-xl border shadow-xl">
                  <img
                    src={Banner}
                    width={800}
                    height={600}
                    alt="Dashboard quản lý trang trại"
                    className="h-auto w-full"
                  />
                </div>
              </div>
            </div>
          </div>
        </section>

        {/* Trusted By Section */}
        <section className="border-y bg-muted/40">
          <div className="container py-12">
            <div className="mb-8 text-center">
              <h2 className="text-xl font-medium text-muted-foreground">
                Được tin dùng bởi các trang trại hàng đầu
              </h2>
            </div>
            <div className="grid grid-cols-2 items-center justify-items-center gap-8 opacity-70 md:grid-cols-3 lg:grid-cols-6">
              {logos.map((i) => (
                <div key={i} className="h-12">
                  <img
                    src={i}
                    width={120}
                    height={120}
                    alt={`Logo đối tác ${i}`}
                    className="h-full w-auto object-contain"
                  />
                </div>
              ))}
            </div>
          </div>
        </section>

        {/* Features Section */}
        <section id="features" className="py-20 md:py-32">
          <div className="container">
            <div className="mx-auto mb-16 max-w-[800px] text-center">
              <h2 className="mb-4 text-3xl font-bold md:text-4xl">
                Tính năng nổi bật
              </h2>
              <p className="text-lg text-muted-foreground">
                Hệ thống quản lý trang trại của chúng tôi cung cấp đầy đủ các
                công cụ cần thiết để vận hành trang trại hiệu quả và bền vững.
              </p>
            </div>

            <div className="grid gap-8 md:grid-cols-2 lg:grid-cols-3">
              {[
                {
                  icon: <BarChart3 className="h-10 w-10 text-green-600" />,
                  title: 'Phân tích dữ liệu',
                  description:
                    'Theo dõi và phân tích dữ liệu sản xuất, năng suất và tài chính với biểu đồ trực quan.'
                },
                {
                  icon: <Calendar className="h-10 w-10 text-green-600" />,
                  title: 'Lập kế hoạch mùa vụ',
                  description:
                    'Lên lịch trình chi tiết cho các hoạt động canh tác, thu hoạch và bảo trì.'
                },
                {
                  icon: <CloudSun className="h-10 w-10 text-green-600" />,
                  title: 'Dự báo thời tiết',
                  description:
                    'Tích hợp dữ liệu thời tiết để lập kế hoạch hoạt động nông nghiệp hiệu quả.'
                },
                {
                  icon: <Tractor className="h-10 w-10 text-green-600" />,
                  title: 'Quản lý thiết bị',
                  description:
                    'Theo dõi tình trạng, lịch bảo trì và hiệu suất của máy móc nông nghiệp.'
                },
                {
                  icon: <Users className="h-10 w-10 text-green-600" />,
                  title: 'Quản lý nhân sự',
                  description:
                    'Phân công công việc, theo dõi giờ làm và quản lý lương cho nhân viên trang trại.'
                },
                {
                  icon: <Leaf className="h-10 w-10 text-green-600" />,
                  title: 'Theo dõi cây trồng',
                  description:
                    'Giám sát sức khỏe cây trồng, lịch sử canh tác và năng suất theo thời gian thực.'
                }
              ].map((feature, index) => (
                <div
                  key={index}
                  className="group relative overflow-hidden rounded-xl border bg-background p-6 transition-all hover:shadow-md"
                >
                  <div className="mb-4">{feature.icon}</div>
                  <h3 className="mb-2 text-xl font-bold">{feature.title}</h3>
                  <p className="text-muted-foreground">{feature.description}</p>
                  <div className="mt-4 flex items-center font-medium text-green-600">
                    <span>Tìm hiểu thêm</span>
                    <ArrowRight className="ml-2 h-4 w-4 transition-transform group-hover:translate-x-1" />
                  </div>
                </div>
              ))}
            </div>
          </div>
        </section>

        {/* How It Works Section */}
        <section id="how-it-works" className="bg-muted/30 py-20 md:py-32">
          <div className="container">
            <div className="mx-auto mb-16 max-w-[800px] text-center">
              <h2 className="mb-4 text-3xl font-bold md:text-4xl">
                Cách thức hoạt động
              </h2>
              <p className="text-lg text-muted-foreground">
                Hệ thống quản lý trang trại của chúng tôi được thiết kế để đơn
                giản hóa quy trình quản lý và tối ưu hóa hiệu quả hoạt động.
              </p>
            </div>

            <div className="grid gap-8 md:gap-12 lg:gap-16">
              {[
                {
                  step: '01',
                  title: 'Thu thập dữ liệu',
                  description:
                    'Hệ thống thu thập dữ liệu từ nhiều nguồn khác nhau như cảm biến IoT, nhập liệu thủ công và tích hợp với các thiết bị nông nghiệp.',
                  img: '/placeholder.svg?height=300&width=600'
                },
                {
                  step: '02',
                  title: 'Phân tích và xử lý',
                  description:
                    'Các thuật toán tiên tiến phân tích dữ liệu thu thập được để cung cấp thông tin chi tiết và đề xuất tối ưu hóa.',
                  img: '/placeholder.svg?height=300&width=600'
                },
                {
                  step: '03',
                  title: 'Trực quan hóa và báo cáo',
                  description:
                    'Dữ liệu được hiển thị dưới dạng biểu đồ và báo cáo trực quan, giúp bạn dễ dàng theo dõi và đưa ra quyết định.',
                  img: '/placeholder.svg?height=300&width=600'
                },
                {
                  step: '04',
                  title: 'Hành động và cải thiện',
                  description:
                    'Dựa trên phân tích và đề xuất, bạn có thể thực hiện các hành động cụ thể để cải thiện hiệu quả và năng suất trang trại.',
                  img: '/placeholder.svg?height=300&width=600'
                }
              ].map((item, index) => (
                <div
                  key={index}
                  className="grid items-center gap-8 md:grid-cols-2"
                >
                  <div
                    className={`space-y-4 ${index % 2 !== 0 ? 'md:order-2' : ''}`}
                  >
                    <div className="inline-flex h-12 w-12 items-center justify-center rounded-full bg-green-100 font-bold text-green-600">
                      {item.step}
                    </div>
                    <h3 className="text-2xl font-bold">{item.title}</h3>
                    <p className="text-muted-foreground">{item.description}</p>
                  </div>
                  <div className={`${index % 2 !== 0 ? 'md:order-1' : ''}`}>
                    <img
                      src={item.img || '/placeholder.svg'}
                      width={600}
                      height={300}
                      alt={item.title}
                      className="h-auto w-full rounded-xl shadow-lg"
                    />
                  </div>
                </div>
              ))}
            </div>
          </div>
        </section>

        {/* Benefits Section */}
        <section
          id="benefits"
          className="bg-gradient-to-b from-white to-green-50 py-20 md:py-32"
        >
          <div className="container">
            <div className="grid items-center gap-12 md:grid-cols-2">
              <div>
                <img
                  src="/placeholder.svg?height=600&width=800"
                  width={800}
                  height={600}
                  alt="Lợi ích của hệ thống quản lý trang trại"
                  className="rounded-xl shadow-xl"
                />
              </div>
              <div className="space-y-8">
                <div>
                  <h2 className="mb-4 text-3xl font-bold md:text-4xl">
                    Tối ưu hóa hoạt động trang trại của bạn
                  </h2>
                  <p className="text-lg text-muted-foreground">
                    Hệ thống của chúng tôi giúp bạn tiết kiệm thời gian, giảm
                    chi phí và tăng năng suất thông qua các công cụ quản lý hiện
                    đại.
                  </p>
                </div>

                <div className="space-y-4">
                  {[
                    'Tăng năng suất lên đến 30% nhờ quy trình tối ưu',
                    'Giảm 25% chi phí vận hành qua việc quản lý hiệu quả',
                    'Tiết kiệm 15-20% chi phí nhân công',
                    'Giảm thiểu lãng phí tài nguyên và thực phẩm',
                    'Cải thiện chất lượng sản phẩm nhờ theo dõi chặt chẽ'
                  ].map((benefit, index) => (
                    <div key={index} className="flex items-start gap-3">
                      <div className="mt-0.5 rounded-full bg-green-100 p-1">
                        <Check className="h-4 w-4 text-green-600" />
                      </div>
                      <span>{benefit}</span>
                    </div>
                  ))}
                </div>

                <Button className="bg-green-600 hover:bg-green-700">
                  Khám phá tất cả lợi ích
                </Button>
              </div>
            </div>
          </div>
        </section>

        {/* Testimonials Section */}
        <section id="testimonials" className="py-20 md:py-32">
          <div className="container">
            <div className="mx-auto mb-16 max-w-[800px] text-center">
              <h2 className="mb-4 text-3xl font-bold md:text-4xl">
                Khách hàng nói gì về chúng tôi
              </h2>
              <p className="text-lg text-muted-foreground">
                Hàng nghìn chủ trang trại đã cải thiện hoạt động của họ với hệ
                thống quản lý của chúng tôi.
              </p>
            </div>

            <div className="grid gap-8 md:grid-cols-3">
              {[
                {
                  quote:
                    'Hệ thống này đã giúp trang trại của tôi tăng năng suất lên 35% chỉ trong 6 tháng đầu sử dụng. Giao diện dễ sử dụng và báo cáo chi tiết giúp tôi đưa ra quyết định tốt hơn.',
                  name: 'Nguyễn Văn A',
                  role: 'Chủ trang trại rau hữu cơ'
                },
                {
                  quote:
                    'Tôi đặc biệt ấn tượng với tính năng lập kế hoạch mùa vụ. Nó giúp tôi tối ưu hóa lịch trình canh tác và tăng hiệu quả sử dụng đất đai.',
                  name: 'Trần Thị B',
                  role: 'Quản lý trang trại cây ăn quả'
                },
                {
                  quote:
                    'Việc quản lý nhân sự và thiết bị trở nên dễ dàng hơn rất nhiều. Tôi có thể theo dõi mọi thứ từ một nơi và tiết kiệm rất nhiều thời gian.',
                  name: 'Lê Văn C',
                  role: 'Giám đốc trang trại chăn nuôi'
                }
              ].map((testimonial, index) => (
                <div
                  key={index}
                  className="rounded-xl border bg-background p-6 shadow-sm"
                >
                  <div className="flex h-full flex-col justify-between">
                    <div>
                      <div className="mb-4 flex gap-0.5">
                        {[1, 2, 3, 4, 5].map((star) => (
                          <svg
                            key={star}
                            className="h-5 w-5 fill-yellow-400"
                            xmlns="http://www.w3.org/2000/svg"
                            viewBox="0 0 24 24"
                          >
                            <path d="M12 17.27L18.18 21l-1.64-7.03L22 9.24l-7.19-.61L12 2 9.19 8.63 2 9.24l5.46 4.73L5.82 21z" />
                          </svg>
                        ))}
                      </div>
                      <p className="mb-4 italic">"{testimonial.quote}"</p>
                    </div>
                    <div className="mt-4 flex items-center gap-3">
                      <div className="flex h-10 w-10 items-center justify-center rounded-full bg-green-100">
                        <span className="font-medium text-green-700">
                          {testimonial.name.charAt(0)}
                        </span>
                      </div>
                      <div>
                        <p className="font-medium">{testimonial.name}</p>
                        <p className="text-sm text-muted-foreground">
                          {testimonial.role}
                        </p>
                      </div>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </section>

        {/* CTA Section */}
        <section
          id="contact"
          className="bg-green-600 py-20 text-white md:py-32"
        >
          <div className="container">
            <div className="grid items-center gap-12 md:grid-cols-2">
              <div className="space-y-6">
                <h2 className="text-3xl font-bold md:text-4xl">
                  Sẵn sàng tối ưu hóa trang trại của bạn?
                </h2>
                <p className="text-lg text-green-50">
                  Liên hệ với chúng tôi ngay hôm nay để tìm hiểu thêm về cách hệ
                  thống quản lý trang trại có thể giúp bạn cải thiện hoạt động
                  và tăng năng suất.
                </p>
                <div className="flex flex-col gap-4 sm:flex-row">
                  <Button size="lg" variant="secondary">
                    Liên hệ ngay
                  </Button>
                  <Button
                    size="lg"
                    variant="outline"
                    className="border-white bg-transparent text-white hover:bg-white/10"
                  >
                    Đặt lịch demo
                  </Button>
                </div>
              </div>
              <div className="rounded-xl bg-white p-6 shadow-xl">
                <h3 className="mb-4 text-xl font-bold text-gray-900">
                  Liên hệ với chúng tôi
                </h3>
                <form className="space-y-4">
                  <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-2">
                      <label
                        htmlFor="first-name"
                        className="text-sm font-medium text-gray-900"
                      >
                        Họ
                      </label>
                      <input
                        id="first-name"
                        className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                        placeholder="Nguyễn"
                      />
                    </div>
                    <div className="space-y-2">
                      <label
                        htmlFor="last-name"
                        className="text-sm font-medium text-gray-900"
                      >
                        Tên
                      </label>
                      <input
                        id="last-name"
                        className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                        placeholder="Văn A"
                      />
                    </div>
                  </div>
                  <div className="space-y-2">
                    <label
                      htmlFor="email"
                      className="text-sm font-medium text-gray-900"
                    >
                      Email
                    </label>
                    <input
                      id="email"
                      type="email"
                      className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                      placeholder="example@example.com"
                    />
                  </div>
                  <div className="space-y-2">
                    <label
                      htmlFor="phone"
                      className="text-sm font-medium text-gray-900"
                    >
                      Số điện thoại
                    </label>
                    <input
                      id="phone"
                      type="tel"
                      className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                      placeholder="0123456789"
                    />
                  </div>
                  <div className="space-y-2">
                    <label
                      htmlFor="message"
                      className="text-sm font-medium text-gray-900"
                    >
                      Tin nhắn
                    </label>
                    <textarea
                      id="message"
                      className="flex min-h-[100px] w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                      placeholder="Hãy cho chúng tôi biết về trang trại của bạn..."
                    />
                  </div>
                  <Button className="w-full bg-green-600 text-white hover:bg-green-700">
                    Gửi tin nhắn
                  </Button>
                </form>
              </div>
            </div>
          </div>
        </section>
      </main>
      <footer className="bg-gray-900 text-gray-300">
        <div className="container py-12 md:py-16">
          <div className="grid grid-cols-2 gap-8 md:grid-cols-4">
            <div className="col-span-2 md:col-span-1">
              <div className="mb-4 flex items-center gap-2">
                <Leaf className="h-6 w-6 text-green-500" />
                <span className="text-xl font-bold text-white">
                  FarmManager
                </span>
              </div>
              <p className="mb-4 text-sm">
                Hệ thống quản lý trang trại toàn diện giúp tối ưu hóa hoạt động
                nông nghiệp và tăng năng suất.
              </p>
              <div className="flex gap-4">
                {['facebook', 'twitter', 'instagram', 'aedin'].map((social) => (
                  <a
                    key={social}
                    href="#"
                    className="text-gray-400 hover:text-white"
                  >
                    <span className="sr-only">{social}</span>
                    <div className="flex h-6 w-6 items-center justify-center rounded-full bg-gray-800">
                      <span className="text-xs">
                        {social.charAt(0).toUpperCase()}
                      </span>
                    </div>
                  </a>
                ))}
              </div>
            </div>

            <div>
              <h3 className="mb-4 font-medium text-white">Hệ thống</h3>
              <ul className="space-y-2 text-sm">
                <li>
                  <a href="#features" className="hover:text-white">
                    Tính năng
                  </a>
                </li>
                <li>
                  <a href="#how-it-works" className="hover:text-white">
                    Cách thức hoạt động
                  </a>
                </li>
                <li>
                  <a href="#benefits" className="hover:text-white">
                    Lợi ích
                  </a>
                </li>
                <li>
                  <a href="#" className="hover:text-white">
                    Tích hợp
                  </a>
                </li>
              </ul>
            </div>

            <div>
              <h3 className="mb-4 font-medium text-white">Hỗ trợ</h3>
              <ul className="space-y-2 text-sm">
                <li>
                  <a href="#" className="hover:text-white">
                    Trung tâm hỗ trợ
                  </a>
                </li>
                <li>
                  <a href="#" className="hover:text-white">
                    Hướng dẫn sử dụng
                  </a>
                </li>
                <li>
                  <a href="#" className="hover:text-white">
                    Cộng đồng
                  </a>
                </li>
                <li>
                  <a href="#contact" className="hover:text-white">
                    Liên hệ
                  </a>
                </li>
              </ul>
            </div>

            <div>
              <h3 className="mb-4 font-medium text-white">Công ty</h3>
              <ul className="space-y-2 text-sm">
                <li>
                  <a href="#" className="hover:text-white">
                    Về chúng tôi
                  </a>
                </li>
                <li>
                  <a href="#" className="hover:text-white">
                    Blog
                  </a>
                </li>
                <li>
                  <a href="#" className="hover:text-white">
                    Tuyển dụng
                  </a>
                </li>
                <li>
                  <a href="#" className="hover:text-white">
                    Báo chí
                  </a>
                </li>
              </ul>
            </div>
          </div>

          <div className="mt-12 flex flex-col items-center justify-between border-t border-gray-800 pt-8 md:flex-row">
            <p className="text-sm">
              © {new Date().getFullYear()} FarmManager. Tất cả các quyền được
              bảo lưu.
            </p>
            <div className="mt-4 flex gap-6 md:mt-0">
              <a href="#" className="text-sm hover:text-white">
                Điều khoản sử dụng
              </a>
              <a href="#" className="text-sm hover:text-white">
                Chính sách bảo mật
              </a>
              <a href="#" className="text-sm hover:text-white">
                Cookie
              </a>
            </div>
          </div>
        </div>
      </footer>
    </div>
  );
}
