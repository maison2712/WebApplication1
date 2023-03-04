<?xml version='1.0' encoding='UTF-8' ?>
<xsl:transform version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:inv="http://laphoadon.gdt.gov.vn/2014/09/invoicexml/v1" xmlns:ds="http://www.w3.org/2000/09/xmldsig#">
  <xsl:strip-space elements="*"/>
  <xsl:decimal-format decimal-separator="," grouping-separator="."/>
  <xsl:template name="string-replace-all">
    <xsl:param name="text"/>
    <xsl:param name="replace"/>
    <xsl:param name="by"/>
    <xsl:param name="spl"/>
    <xsl:choose>
      <xsl:when test="contains($text,$replace)">
        <xsl:value-of select="substring-before($text,$replace)"/>
        <xsl:value-of select="$by"/>
        <br/>
        <xsl:value-of select="$spl"/>
        <xsl:call-template name="string-replace-all">
          <xsl:with-param name="text" select="substring-after($text,$replace)"/>
          <xsl:with-param name="replace" select="$replace"/>
          <xsl:with-param name="by" select="$by"/>
          <xsl:with-param name="spl" select="$spl"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$text"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  <xsl:template name="tokenize">
    <xsl:param name="pText"/>
    <xsl:if test="string-length($pText)">
      <xsl:choose>
        <xsl:when test="contains($pText,',')">
          <xsl:variable name="text">
            <xsl:value-of select="substring-before($pText, ',')"/>
          </xsl:variable>
          <xsl:choose>
            <xsl:when test="substring($text,1,3) = 'CN='">
              <xsl:value-of select="substring-after($text, 'CN=')"/>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name="tokenize">
                <xsl:with-param name="pText" select=
       "substring-after($pText, ',')"/>
              </xsl:call-template>
            </xsl:otherwise>
          </xsl:choose>
        </xsl:when>
        <xsl:otherwise>
          <xsl:if test="substring($pText,1,3) = 'CN='">
            <xsl:value-of select="substring-after($pText, 'CN=')"/>
          </xsl:if>
        </xsl:otherwise>
      </xsl:choose>
    </xsl:if>
  </xsl:template>
  <xsl:template name="loop">
    <xsl:param name="var"></xsl:param>
    <xsl:choose>
      <xsl:when test="$var &lt; 7  and $var &gt; 0">
        <tr>
          <td align="center" class= "boxSmall itemNormal">
            <font class="labelNormal" style="color:white">.</font>
          </td>
          <td align="left" class= "boxSmall itemNormal">
			<font class="labelNormal" ></font>
          </td>
          <td align="center" class= "boxSmall itemNormal">
		  <font class="labelNormal" ></font>
          </td>
          <td align="right" class= "boxSmall itemNormal">
			<font class="labelNormal" ></font>
          </td>
          <td align="right" class= "boxSmall itemNormal">
		  <font class="labelNormal" ></font>
          </td>
          <td align="right" class= "boxSmall itemNormal">
		  <font class="labelNormal" ></font>
          </td>
        </tr>
        <xsl:call-template name="loop">
          <xsl:with-param name="var">
            <xsl:number value="number($var)+1" />
          </xsl:with-param>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
 <xsl:decimal-format decimal-separator="." grouping-separator="," name="decimalFormat"/>
<xsl:decimal-format decimal-separator="." grouping-separator="," name="us"/>
<xsl:decimal-format decimal-separator="," grouping-separator="." name="european"/>
<xsl:decimal-format NaN="Not a Number" decimal-separator="." digit="#" grouping-separator="," infinity="INFINITY" minus-sign="-" name="example" pattern-separator=";" per-mille="m" percent="%" zero-digit="0"/>
<xsl:template match="/HDon">
    <html>
      <head>
        <style>
          tbody{
          }
          .header{
          vertical-align:top
          }
          .invoiceName{
          font-weight:bold;
          font-size:18px;
          }
          .titleInvoice{
          font-weight:bold;
          font-style: normal;
          font-size:16px;
          }
          .serif {
          font-family: "Times New Roman", Times, serif;
          }

          .sansserif {
          font-family: Arial, Helvetica, sans-serif;
          }
          .image-box {
          text-align:center;
          }
          .image-box img {
          <!--opacity: 0.9;-->
          width: 350px;
          background-image: none;
          background-repeat: no-repeat;
          background-position: center center;
          background-size: cover;
          margin-top:300px;
          margin-bottom: 100px;
          }

          .image-box img[src=""] {
          display: none;
          }

          .image-qrcode img {
          max-height: 110px;
          max-width: 100%;
          }
          .image-qrcode img[src=""] {
          display: none;
          }

          .watermark {
          top: 0;
          left: 0;
          bottom: 0;
          right: 0;
          position: absolute;
          z-index: -2;
          margin-left:auto;
          margin-right:auto;
          width:400px;
          margin-top: 0px;
          }

          .itemNormal{
          font-weight: normal;
          padding : 2px 2px 2px 2px;
          font-size: 10pt
          }

          .itemBold{
          font-weight:bold;
          /*vertical-align:top;*/
          padding : 2px 2px 2px 2px;
          font-size: 10pt
          }
          .labelNormal{
          padding : 2px 2px 2px 2px;
          font-size: 10pt
          }

          .labelItalic{
          padding : 2px 2px 2px 2px;
          font-style: italic;
          color: #000000;
          font-size: 10pt
          }

          .labelItalicNormal{
          padding : 2px 2px 2px 2px;
          font-style: italic;
          font-weight: normal;
          color: #000000;
          font-size: 10pt
          }

          .labelBold{
          font-weight:bold;
          /*vertical-align:top;*/
          padding : 2px 2px 2px 2px;
          font-size: 10pt
          }



          .boxLarge{
          margin-left:auto;
          margin-right:auto;
          border-collapse: collapse;
          padding : 5px 5px 5px 5px;
          border: 3px double;
          width:838.267px;
          }
          .boxSmall{
          border-collapse: collapse;
          padding : 5px 5px 5px 5px;
          border: 1px solid;
          }
          .boxSmallTable{
          border-spacing: 0px;
          padding : 0px 0px 0px 0px;
          border: 1px solid;
          }
          .dataInfoInvoice{
          vertical-align:top;
          font-weight:bold;
          padding : 2px 2px 2px 2px
          }
          <!--Bat buoc phai co - dau hieu nhan biet de change color-->
          <!--Start_color-->
          .invoiceName{
          color: #000000;
          }
          .invoiceNameItalic{
          color: #000000;
          font-style: italic;
          }
          .titleInvoice{
          color: #000000;
          }
          .itemNormal{
          color: #000000;
          }
          .itemBold{
          color: #000000;
          }
          .labelNormal{
          color: #000000;
          }
          .labelBold{
          color: #000000;
          }
          .boxLarge{
          color: #000000;
          border-style: solid;
          border-width: medium;
          }
          .boxSmall{
          color: #000000;
          }


          .borderBottom{
          border-bottom: 2px solid #4C3F57;
          }
          .BG {
          <!--opacity: 0.3;-->
          background-image: url(signature.png);
          background-repeat: no-repeat;
          background-position: center top;
          background-size: 200px 70px;
          }
          img[src=""] {
          display: none;
          }
          <!--End_custom_nuoc_lai_chau
                    background-image: url("background.jpg");
                    z-index: -16 !important;
                    -->
        </style>
      </head>
      <body >
        <table  id='section-to-print' ALIGN="center" class = "serif boxLarge" style="background-image:url(watermark.png); background-repeat:no-repeat;background-position: center 300px;">
          <tr class = "borderBottom">
            <td width="15%" align="left" style="vertical-align:top">
              <img src="logo.png" style="max-height: 50px; max-width: 100%;" align="left"/>
              <br/>
            </td>
            <td style="vertical-align:top" width="35%">
				<table width="100%">
					<tr>
						<td align="left" style="padding-bottom:6px" colspan="3">
							<font class="labelNormal" style="letter-spacing:-0.7pt">
								<xsl:value-of select="DLHDon/NDHDon/NBan/Ten"/>
							</font>
						</td>
					</tr>
					<tr>
						<td align="left" width="20%">
							<font></font>
						</td>
						<td align="center" class="boxSmall" style="padding: 1px">
							<font class="labelBold">Mã số (code): </font>
							<font class="labelBold">
								<xsl:variable name = "sellerTaxCodeLength" select = "string-length(/HDonData/inv:seller/inv:sellerTaxCode)"/>
								<xsl:variable name = "sellerTaxCode" select = "/HDonData/inv:seller/inv:sellerTaxCode"/>
								<xsl:value-of select="$sellerTaxCode"/>
							</font>
						</td>
						<td align="left" width="20%">
							<font></font>
						</td>
					</tr>
				</table>
            </td>
            <td width="50%" style="vertical-align:top;">
              <!--<table align="right" class= "boxSmall dataInfoInvoice" style = "border: none !important;">-->
              <table align="left">
                <tr style="vertical-align:top"> 
                  <td align="left" colspan="2">
                    <font class="labelNormal" >
						<xsl:value-of select="DLHDon/NDHDon/NBan/DChi"/>
					</font>
                  </td> 
                </tr>
				<tr> 
                  <td align="left" colspan="2">
                    <font class="labelNormal" >
						4th floor, Harbour View office building, No 12 Tran Phu Street, May To Ward, Ngo Quyen Dist., Hai Phong City, Viet Nam.
					</font>
                  </td> 
                </tr>
				<tr style="vertical-align:top"> 
                  <td align="left">
					<font class="labelNormal" >
						Tel: <xsl:value-of select="DLHDon/NDHDon/NBan/SDThoai"/>
					</font>
                  </td> 
				  <td align="left">
					<font class="labelNormal" >
						Fax: <xsl:value-of select="DLHDon/NDHDon/NBan/Fax"/>
					</font>
                  </td> 
                </tr>
              </table>
            </td>
          </tr>
		  <tr>
            <td align="center" class="header" style="color:#000000" colspan="3">
              <font class="invoiceName" style="font-size: 18pt">
                HÓA ĐƠN GIÁ TRỊ GIA TĂNG
              </font>
              <br/>
              <font class="invoiceNameItalic" style="font-size: 16pt">
                (VAT INVOICE)
              </font>
              <br/>
              <font class="titleInvoice">Bản thể hiện của hóa đơn điện tử</font>
              <br/>
              <font class="labelItalic">
                (Electronic invoice display)
              </font>
            </td>
          </tr>
			<tr>
				<td colspan="3">
					<table width="100%">
						<tr>
							<td width="60%">
								<table width="100%">
									<tr>
										<td>
											<font class="labelBold" >Tên khách hàng</font>
											<font class="labelNormal" >(Client's name): </font>
											<font class="labelNormal">
												<xsl:value-of select="DLHDon/NDHDon/NMua/Ten"/>
											</font>
										</td>
									</tr>
									<tr>
										<td>
											<font class="labelBold" >Địa chỉ</font>
											<font class="labelNormal" >(Address): </font>
											<font class="labelBold">
												<xsl:value-of select="DLHDon/NDHDon/NMua/DChi"/>
											</font>
										</td>
									</tr>
									<tr>
										<td>
											<font class="labelBold" >Mã số</font>
											<font class="labelNormal" >(Code): </font>
											<font class="labelNormal">
												<xsl:value-of select="DLHDon/NDHDon/NMua/MST"/>
											</font>
										</td>
									</tr>
									<tr>
										<td>
											<font class="labelBold" >Hình thức thanh toán</font>
											<font class="labelNormal" >(Mode of payment): </font>
											<font class="labelNormal">
												<xsl:value-of select="DLHDon/TTChung/HTTToan"/>
											</font>
										</td>
									</tr>
								</table>
							</td>
							<td width="40%">
								<table width="100%">
									<!-- <tr> -->
										<!-- <td> -->
											<!-- <font class="labelBold" >Mẫu số:</font> -->
											<!-- <font class="labelBold" ></font> -->
											<!-- <font class="labelBold"> -->
												<!-- <xsl:value-of select="inv:DLHDon/TTChung/KHMSHDon"/> -->
											<!-- </font> -->
										<!-- </td> -->
									<!-- </tr> -->
									<tr>
										<td>
											<font class="labelBold" >Ký hiệu</font>
											<font class="labelBold" >/Series No.:</font>
											<font class="labelNormal">
												<xsl:value-of select="DLHDon/TTChung/KHMSHDon"/><xsl:value-of select="DLHDon/TTChung/KHHDon"/>
											</font>
										</td>
									</tr>
									<tr>
										<td>
											<font class="labelBold" >Số hóa đơn</font>
											<font class="labelBold" >/Invoice No.: </font>
											<font class="labelNormal" font-size="12pt">
												<xsl:value-of select="DLHDon/TTChung/SHDon"/>
											</font>
										</td>
									</tr>
									<tr>
										<td>
											<font class="labelBold" >Ngày</font>
											<font class="labelBold" >/Date: </font>
											<font class="labelNormal">
												<xsl:choose>
													<xsl:when test="DLHDon/TTChung/NLap !='null'">
														  <xsl:value-of select="concat(substring(DLHDon/TTChung/NLap, 9, 2),'/',substring(DLHDon/TTChung/NLap, 6, 2),'/',substring(DLHDon/TTChung/NLap, 1, 4))"/>
													</xsl:when>
													<xsl:otherwise>
													</xsl:otherwise>
												</xsl:choose>
											</font>
										</td>
									</tr>
								</table>
							</td>
						</tr>
					</table>
				</td>
			</tr>
          <tr>
            <td colspan="3">
              <table width="100%" class= "boxSmallTable">
                <tr width="100%">
                  <!--<td width="5%" align="center" class= "boxSmall labelBold">STT <br/>(No.)</td>-->
                  <td width="5%" align="center" class= "boxSmall"  style="text-align: center">
                    <font class="labelBold" >STT</font>
                    <br/>
                    <font class="labelItalicNormal">(No.)</font>
                  </td>
                  <!--<td width="32%" align="center" class= "boxSmall labelBold">Tên hàng hóa, dịch vụ <br/> (Description)</td>-->
                  <td align="center" class= "boxSmall"  style="text-align: center">
                    <font class="labelBold" >Tên hàng hóa, dịch vụ</font>
                    <br/>
                    <font class="labelItalicNormal" >(Description)</font>
                  </td>
                  <!--<td width="11%" align="center" class= "boxSmall labelBold">Đơn vị tính <br/>(Unit)</td>-->
                  <td width="11%" align="center" class= "boxSmall"  style="text-align: center">
                    <font class="labelBold" >Đơn vị tính</font>
                    <br/>
                    <font class="labelItalicNormal" >(Unit)</font>
                  </td>
                  <!--<td width="10%" align="center" class= "boxSmall labelBold">Số lượng <br/>(Quantity)</td>-->
                  <td width="12%" align="center" class= "boxSmall"  style="text-align: center">
                    <font class="labelBold" >Số lượng</font>
                    <br/>
                    <font class="labelItalicNormal" >(Quantity)</font>
                  </td>
                  <!--<td width="13%" align="center" class= "boxSmall labelBold">Đơn giá <br/>(Unit price)</td>-->
                  <td width="13%" align="center" class= "boxSmall"  style="text-align: center">
                    <font class="labelBold" >Đơn giá</font>
                    <br/>
                    <font class="labelItalicNormal" >(Unit price)</font>
                  </td>
                  <!--<td width="13%" align="center" class= "boxSmall labelBold">Thành tiền <br/>(Amount)</td>-->
                  <td width="13%" align="center" class= "boxSmall"  style="text-align: center">
                    <font class="labelBold" >Thành tiền</font>
                    <br/>
                    <font class="labelItalicNormal" >(Amount)</font>
                  </td>
                </tr>
                <tr width="100%">
                  <td align="center" class= "boxSmall labelBold"  style="text-align: center">1</td>
                  <td align="center" class= "boxSmall labelBold" style="text-align: center">2</td>
                  <td align="center" class= "boxSmall labelBold"  style="text-align: center">3</td>
                  <td align="center" class= "boxSmall labelBold" style="text-align: center">4</td>
                  <td align="center" class= "boxSmall labelBold" style="text-align: center">5</td>
                  <td align="center" class= "boxSmall labelBold" style="text-align: center">6 = 4 x 5</td>
                </tr>
                <xsl:for-each select="DLHDon/NDHDon/DSHHDVu/HHDVu">
                  <tr>
                    <td align="center" class= "boxSmall itemNormal">
                      <xsl:choose>
                        <xsl:when test="STT > 0">
                           <xsl:value-of select="STT"/>
                        </xsl:when>
                        <xsl:otherwise>
                          <font class="labelNormal" ></font>
                        </xsl:otherwise>
                      </xsl:choose>                    
                    </td>
                    <td align="left" class= "boxSmall itemNormal">
                      <xsl:value-of select="THHDVu"/>
                    </td>
                    <td align="center" class= "boxSmall itemNormal">
                      <xsl:value-of select="DVTinh"/>
                    </td>
                    <td align="right" class= "boxSmall itemNormal">
                      <xsl:if test="SLuong != 'null' and SLuong != '' and SLuong >= 0">
                        <xsl:value-of select="format-number(SLuong, '###,##0.##', 'us')"/>
                      </xsl:if>
                    </td>
                    <td align="right" class= "boxSmall itemNormal">
                      <xsl:if test="DGia != 'null' and DGia != '' and DGia >= 0">
                        <xsl:value-of select="format-number(DGia, '###,##0.##', 'us')"/>
                      </xsl:if>
                    </td>
                    <td align="right" class= "boxSmall itemNormal">
                      <xsl:if test="ThTien != 'null' and ThTien != '' and ThTien >= 0">
                        <xsl:value-of select="format-number(ThTien, '###,##0.##', 'us')"/>
                      </xsl:if>
                    </td>
                  </tr>
				  <xsl:if test="TTKhac/TTin[TTruong='Ghi chú dòng']/DLieu != 'null' and TTKhac/TTin[TTruong='Ghi chú dòng']/DLieu != ''">
						  <tr>
							<td align="center" class= "boxSmall itemNormal">                
							</td>
							<td align="left" class= "boxSmall itemNormal">
							  <xsl:value-of select="TTKhac/TTin[TTruong='Ghi chú dòng']/DLieu"/>
							</td>
							<td align="center" class= "boxSmall itemNormal">
							 
							</td>
							<td align="right" class= "boxSmall itemNormal">
							  
							</td>
							<td align="right" class= "boxSmall itemNormal">
							  
							</td>
							<td align="right" class= "boxSmall itemNormal">
							  
							</td>
						  </tr>
				  </xsl:if>
                </xsl:for-each>
                <xsl:call-template name="loop">
                  <xsl:with-param name="var">
                    <xsl:value-of select="count(//DLHDon/NDHDon/DSHHDVu/HHDVu)"/>
                  </xsl:with-param>
                </xsl:call-template>
                <tr>
					<td align="right" class= "boxSmall">
						<font class="labelBold" ></font>
                  </td>
				  <td align="right" class= "boxSmall" style="border-right:none"> 
						<font class="labelBold" ></font>
                  </td>
                  <td align="left" colspan="3" class= "boxSmall" style="border-left:none">
                    <font class="labelBold" >GIÁ BÁN</font>
                    <font class="labelBold" >(CHƯA CÓ THUẾ): </font>
					<br />
                    <font class="labelNormal" >The selling price (exclusive of VAT)</font>
                  </td>
                  <td align="right" class= "boxSmall itemNormal">
                    <xsl:if test="DLHDon/NDHDon/TToan /TgTCThue != 'null' and DLHDon/NDHDon/TToan /TgTCThue >= 0">
                      <xsl:value-of select="format-number(DLHDon/NDHDon/TToan /TgTCThue, '###,##0.##', 'us')"/>
                    </xsl:if>
                  </td>
                </tr>
                <xsl:choose>
                  <xsl:when test="DLHDon/NDHDon/TToan/THTTLTSuat/LTSuat != 'null'">
                    <xsl:for-each select="DLHDon/NDHDon/TToan/THTTLTSuat/LTSuat">
                      <tr>
						<td align="left" class= "boxSmall labelNormal">
						</td>
                        <td align="left"  colspan="1" class= "boxSmall labelNormal" style="border-right:none">
                          <font style = "" class="labelBold">Thuế suất GTGT</font>
                          <font style = "" class="labelItalic">(VAT rate):</font>
                          <font style = "" class="labelNormal">

                            <xsl:choose>
                              <xsl:when test="TSuat != 'null' and (TSuat = '0%' or TSuat = '5%' or TSuat = '10%')">
                                <xsl:choose>
                                  <xsl:when test="contains(DLHDon/TTChung/TTHDLQuan/GChu,'XXX')">
                                    xxx
                                  </xsl:when>
                                  <xsl:otherwise>
                                    <xsl:value-of select="TSuat"/>
                                  </xsl:otherwise>
                                </xsl:choose>
                              </xsl:when>
                              <xsl:otherwise>
                                .....\.....%
                              </xsl:otherwise>
                            </xsl:choose>
                          </font>
                        </td>
                        <td align="left"  colspan="3" class= "boxSmall labelNormal" style="border-left:none">
                          <font style= "" class="labelBold">Tiền thuế GTGT</font>
						  <br />
                          <font style= "" class="labelNormal">The VAT</font>
                        </td>
                        <xsl:choose>
                          <xsl:when test="TSuat != 'null' and (TSuat = '0%' or TSuat = '5%' or TSuat = '10%')">
                            <td align="right" colspan="1" class= "boxSmall itemNormal">
                              <xsl:if test="TThue != 'null' and TThue >= 0">
                                <font class = "itemNormal">
                                  <xsl:value-of select="format-number(TThue, '###,##0.##', 'us')"/>
                                </font>
                              </xsl:if>
                            </td>
                          </xsl:when>
                          <xsl:otherwise>
                            <td align="right" colspan="1" class= "boxSmall itemNormal">
                              .....\.....
                            </td>
                          </xsl:otherwise>
                        </xsl:choose>
                      </tr>
                    </xsl:for-each>
                  </xsl:when>
                  <xsl:otherwise>
                    <tr>
						<td align="left" class= "boxSmall labelNormal">
						</td>
                      <td align="left"  colspan="1" class= "boxSmall labelNormal" stype="border-right:none">
                        <font style = "" class="labelBold">Thuế suất GTGT</font>
                        <font style = "" class="labelItalic">(VAT rate):</font>
                        <font style = "" class="labelNormal">
                          ..........%
                        </font>
						<br />
                        <font class="labelNormal" style="color:white">...</font>
                      </td>
                      <td align="left"  colspan="3" class= "boxSmall labelNormal" style="border-left:none">
                        <font style= "" class="labelBold">Tiền thuế GTGT </font>
						<br />
                        <font style= "" class="labelNormal">The VAT</font>

                      </td>
                      <td align="right" colspan="1" class= "boxSmall itemNormal">
                      </td>
                    </tr>
                  </xsl:otherwise>
                </xsl:choose>
                <tr>
					<td align="center" colspan="1" class= "boxSmall labelNormal">
					</td>
					<td align="center" colspan="1" class= "boxSmall labelNormal" style="border-right:none">
					</td>
                  <td align="left" colspan="3" class= "boxSmall" style="border-left:none">
                    <font class="labelBold" >TỔNG CỘNG TIỀN THANH TOÁN</font>
					<br />
                    <font class="labelNormal" >The total value to be paid (inclusive of VAT)</font>
                  </td>
                  <td align="right" class= "boxSmall itemNormal">
                    <xsl:if test="DLHDon/NDHDon/TToan /TgTTTBSo != 'null' and DLHDon/NDHDon/TToan /TgTTTBSo >= 0">
                      <xsl:value-of select="format-number(DLHDon/NDHDon/TToan /TgTTTBSo, '###,##0.##', 'us')"/>
                    </xsl:if>
                  </td>
                </tr>
                <tr>
                  <td align="left" colspan="6" class= "boxSmall">
                    <font class="labelBold" >Số tiền viết bằng chữ</font>
                    <font class="labelItalic" >(Amount in words):</font>
                    <font class = "itemNormal">
                      <xsl:value-of select="DLHDon/NDHDon/TToan /TgTTTBChu"/>
                    </font>
                  </td>
                </tr>
              </table>
            </td>
          </tr>
			<tr >
				<td colspan="3">
              <table width="100%">
                <tr>
			  <td align="left"  class= "">
				<font class="labelNormal" >Thanh toán bằng điện chuyển tiền đến ngân hàng của chúng tôi</font>
				<br />
				<font class="labelNormal" >Payment by telegraphic tranfer to our banker</font>
				<br />
				<div style="padding-left:40px">
					<font class="labelNormal" >Bank name: </font>
					<font class = "itemNormal">
					  <xsl:value-of select="DLHDon/NDHDon/NBan/TNHang"/>
					</font>
					<br />
					<font class="labelNormal" >Account No.: </font>
					<font class = "itemNormal">
					  <xsl:value-of select="DLHDon/NDHDon/NBan/STKNHang"/>
					</font>
					<br />
					<font class="labelNormal" >Account name: </font>
					<font class = "itemNormal">
					  <xsl:value-of select="DLHDon/NDHDon/NBan/Website"/>
					</font>
				</div>
			  </td>
			  <td align="right" class= "">
				<div id="qrcode" style="max-width: 100%; max-height: 110px" class="img-qrcode">
<img src="qrcode.png" style="max-height: 110px; max-width: 100%;" align="middle"/>
</div>
			  </td>
			  </tr>
			  </table>
			  </td>
			</tr>
          <tr>
            <td colspan="3">
              <table width="100%">
                <tr>
                  <td align="center" width="50%" style="vertical-align:top">
                    <font class="labelBold" text-align="top">Người mua hàng</font>
                    <font class="labelItalic" >(Buyer)</font>
                  </td>
                  <td  align="center" width="50%">
                    <font class="labelBold" >Người bán hàng</font>
                    <font class="labelItalic" >(Seller)</font>
                  </td>
                </tr>
                <tr>
                  <td align="center">
                    <xsl:if test="not((//*[local-name()='X509SubjectName'])[2]) = false() and (//*[local-name()='X509SubjectName'])[2] != ''">
                      <div class="BG">
                        <div style="height: 30px"  ></div>

                        <font class="labelBold" style="font-weight:bold;color: #FF0000;word-wrap:break-word">
                          Ký bởi <xsl:call-template name="tokenize">
                            <xsl:with-param name="pText" select="(//*[local-name()='X509SubjectName'])[2]"/>
                          </xsl:call-template>
                        </font>

                        <div style="height: 10px"  ></div>
                      </div>
                    </xsl:if>
                  </td>
                  <td align="center" width="50%">
                    <div class="BG">
                      <div style="height: 30px"  ></div>
                      <xsl:if test="DLHDon/NDHDon/NBan/Ten != 'null'">
                        <font class="labelBold" style="font-weight:bold;color: #FF0000;word-wrap:break-word">
                          Ký bởi <xsl:call-template name="tokenize">
                            <xsl:with-param name="pText" select="(//*[local-name()='X509SubjectName'])[1]"/>
                          </xsl:call-template>
                        </font>
                        <br/>
                        <font class="labelBold" style="font-weight:bold;color: #FF0000;word-wrap:break-word">
                          Ký ngày
                          <xsl:if test="DLHDon/TTChung/NLap != 'null' and DLHDon/TTChung/NLap != ''">
                            <xsl:value-of select="concat(substring(DLHDon/TTChung/NLap, 9, 2),'/',substring(DLHDon/TTChung/NLap, 6, 2),'/',substring(DLHDon/TTChung/NLap, 1, 4))"/>
                          </xsl:if>
                        </font>
                      </xsl:if>
                      <div style="height: 10px"  ></div>
                    </div>
                  </td>
                </tr>
              </table>
            </td>
          </tr>
          <tr>
            <td colspan="3">
              <table width="100%" style="border-top: 2pt dotted; margin-top: 10mm; font-size: 10pt">
                <tr>
                  <td align="center">
                    <font font-size="7pt" class="labelItalic"> Đơn vị cung cấp dịch vụ Hóa đơn điện tử: Tập đoàn Công nghiệp - Viễn thông Quân đội (Viettel), MST: 0100109106 </font>
                  </td>
                </tr>
                <tr>
                  <td align="center" >
                    <font font-size="7pt" class="labelItalic">
                      Tra cứu hóa đơn điện tử tại Website: <xsl:choose>
                        <xsl:when test="not(DLHDon/NDHDon/NBan/TTKhac/TTin[TTruong='Link tra cứu người bán']/DLieu) = false() and DLHDon/NDHDon/NBan/TTKhac/TTin[TTruong='Link tra cứu người bán']/DLieu != ''">
                          <xsl:value-of select="DLHDon/NDHDon/NBan/TTKhac/TTin[TTruong='Link tra cứu người bán']/DLieu"/>
                        </xsl:when>
                        <xsl:otherwise>
                          https://vinvoice.viettel.vn/utilities/invoice-search
                        </xsl:otherwise>
                      </xsl:choose>, Mã số bí mật:
                    </font>
                    <font font-size="7pt" class = "itemNormal">
                      <xsl:value-of
                          select ="DLHDon/TTChung/TTKhac/TTin[TTruong='Mã số bí mật']/DLieu" />
                    </font>
                  </td>
                </tr>
              </table>
            </td>
          </tr>
        </table>
      </body>
    </html>
  </xsl:template>
</xsl:transform>
